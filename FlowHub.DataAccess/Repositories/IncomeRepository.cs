
namespace FlowHub.DataAccess.Repositories;

public class IncomeRepository : IIncomeRepository
{
    LiteDatabaseAsync db;
    IMongoDatabase DBOnline;
    public List<IncomeModel> OfflineIncomesList { get; set; }
    public List<IncomeModel> OnlineIncomesList { get; set; }

    IMongoCollection<IncomeModel> AllIncomesOnline;
    ILiteCollectionAsync<IncomeModel> AllIncomes;

    readonly IDataAccessRepo dataAccessRepo;
    readonly IUsersRepository usersRepo;
    readonly IOnlineCredentialsRepository onlineDataAccessRepo;

    private const string incomesDataCollectionName = "Incomes";

    bool IsBatchUpdate;
    public event Action OfflineIncomesListChanged;

    public IncomeRepository(IDataAccessRepo dataAccess, IOnlineCredentialsRepository onlineRepository, IUsersRepository userRepository)
    {
        dataAccessRepo= dataAccess;
        usersRepo= userRepository;
        onlineDataAccessRepo = onlineRepository;
    }

    async Task<LiteDatabaseAsync> OpenDB()
    {
        db = dataAccessRepo.GetDb();
        AllIncomes = db.GetCollection<IncomeModel>(incomesDataCollectionName);
        await AllIncomes.EnsureIndexAsync(x => x.Id);
        return db;
    }

    public async Task<List<IncomeModel>> GetAllIncomesAsync()
    {
        if(OfflineIncomesList is not null)
        {
            return OfflineIncomesList;
        }
        try
        {
            await OpenDB();
            string userId = usersRepo.OfflineUser.Id;
            string userCurrency = usersRepo.OfflineUser.UserCurrency;
            if (usersRepo.OfflineUser.UserIDOnline != string.Empty)
            {
                userId = usersRepo.OfflineUser.UserIDOnline;
            }
            OfflineIncomesList = await AllIncomes.Query()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.UpdatedDateTime)
                .ToListAsync();
            db.Dispose();
            return OfflineIncomesList;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return Enumerable.Empty<IncomeModel>().ToList();
        }
    }

    async Task LoadOnlineDB()
    {
        if(Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            return;
        }
        if(OnlineIncomesList is not null)
        {
            return;
        }
        if (DBOnline is null)
        {
            onlineDataAccessRepo.GetOnlineConnection();
            DBOnline = onlineDataAccessRepo.OnlineMongoDatabase;
        }

        if(usersRepo.OnlineUser is null)
        {
            try
            {
                var filterUserCredentials = Builders<UsersModel>.Filter.Eq("Email", usersRepo.OfflineUser.Email) &
                    Builders<UsersModel>.Filter.Eq("Password", usersRepo.OfflineUser.Password);
                usersRepo.OnlineUser = await DBOnline.GetCollection<UsersModel>("Users").Find(filterUserCredentials).FirstOrDefaultAsync();
                if (usersRepo.OnlineUser is null)
                {
                    await Shell.Current.DisplayAlert("Error", "User not found", "Ok");
                    Debug.WriteLine("User not found");
                    return;
                }
                else
                {
                    usersRepo.OfflineUser.UserIDOnline = usersRepo.OnlineUser.Id;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception Message {ex.Message}");
            }
        }

        var filtersIncome = Builders<IncomeModel>.Filter.Eq("UserId", usersRepo.OnlineUser.Id) &
            Builders<IncomeModel>.Filter.Eq("Currency", usersRepo.OfflineUser.UserCurrency);

        AllIncomesOnline ??= DBOnline?.GetCollection<IncomeModel>(incomesDataCollectionName);
       
        OnlineIncomesList = await AllIncomesOnline.Find(filtersIncome).ToListAsync();
    }

    bool IsSyncing;
    public async Task SynchronizeIncomesAsync()
    {
        try
        {
            await GetAllIncomesAsync();
            await LoadOnlineDB();
            if (usersRepo.OnlineUser is null)
            {
                return;
            }

            if (OnlineIncomesList.Count < 1 && OfflineIncomesList.Count < 1)
            {
                return; //no need to sync
            }
            IsSyncing = true;
            IsBatchUpdate = true;

            Dictionary<string, IncomeModel> OfflineIncomeDict = OfflineIncomesList.ToDictionary(x => x.Id, x => x);
            Dictionary<string, IncomeModel> OnlineIncomeDict = OnlineIncomesList.ToDictionary(x => x.Id, x => x);

            foreach (var itemId in OfflineIncomeDict.Keys.Intersect(OnlineIncomeDict.Keys))
            {
                var offlineItem = OfflineIncomeDict[itemId];
                var onlineItem = OnlineIncomeDict[itemId];

                if (offlineItem.UpdatedDateTime > onlineItem.UpdatedDateTime)
                {
                    await UpdateIncomeOnlineAsync(offlineItem);
                }
                else if (offlineItem.UpdatedDateTime < onlineItem.UpdatedDateTime)
                {
                    await UpdateIncomeAsync(onlineItem);
                }
            }

            foreach (var itemID in OfflineIncomeDict.Keys.Except(OnlineIncomeDict.Keys))
            {
                await AddIncomeOnlineAsync(OfflineIncomeDict[itemID]);
                OnlineIncomesList.Add(OfflineIncomeDict[itemID]);
            }

            foreach (var itemID in OnlineIncomeDict.Keys.Except(OfflineIncomeDict.Keys))
            {
                await AddIncomeAsync(OnlineIncomeDict[itemID]);
                OfflineIncomesList.Add(OnlineIncomeDict[itemID]);
            }

            await usersRepo.UpdateUserOnlineGetSetLatestValues(usersRepo.OnlineUser);
            IsSyncing = false;
            IsBatchUpdate = false;
            OfflineIncomesListChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Exception Message : " + ex.Message);
        }
    }

    public async Task<bool> AddIncomeAsync(IncomeModel newIncome)
    {
        try
        {
            using (db = await OpenDB())
            {
                if (await AllIncomes.InsertAsync(newIncome) is not null)
                {
                    OfflineIncomesList.Add(newIncome);
                    Debug.WriteLine("Income inserted Locally");
                    if (!IsBatchUpdate)
                    {
                        OfflineIncomesListChanged?.Invoke();
                    }

                    if (!IsSyncing && Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        OnlineIncomesList.Add(newIncome);
                        await AddIncomeOnlineAsync(newIncome);
                    }
                    return true;
                }
                else
                {
                    Debug.WriteLine("Error while adding Income");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to add local income: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> UpdateIncomeAsync(IncomeModel income)
    {
        try
        {
            using (db = await OpenDB())
            {
                if (await AllIncomes.UpdateAsync(income))
                {
                    Debug.WriteLine("Income updated Locally");

                    int index = OfflineIncomesList.FindIndex(x => x.Id == income.Id);
                    OfflineIncomesList[index] = income;
                    if (!IsBatchUpdate)
                    {
                        OfflineIncomesListChanged?.Invoke();
                    }

                    if (!IsSyncing && Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        await UpdateIncomeOnlineAsync(income);
                    }
                    return true;
                }
                else
                {
                    Debug.WriteLine("Error while updating Income");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to handle local income: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteIncomeAsync(IncomeModel income)
    {
        income.IsDeleted = true;

        try
        {
            using (db = await OpenDB())
            {
                if (await AllIncomes.UpdateAsync(income))
                {
                    OfflineIncomesList.Remove(income);
                    Debug.WriteLine("Income deleted Locally");
                    if (!IsBatchUpdate)
                    {
                        OfflineIncomesListChanged?.Invoke();
                    }

                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        await DeleteIncomeOnlineAsync(income);
                    }
                    return true;
                }
                else
                {
                    Debug.WriteLine("Error while deleting Income");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to handle local income: " + ex.Message);
            return false;
        }
    }
 /*--------- SECTION FOR ONLINE CRUD OPERATIONS----------*/

    async Task AddIncomeOnlineAsync(IncomeModel income)
    {
        await AllIncomesOnline?.InsertOneAsync(income);
        Debug.WriteLine("Income added online");
    }

    async Task UpdateIncomeOnlineAsync(IncomeModel inc)
    {
        await AllIncomesOnline?.ReplaceOneAsync(x => x.Id == inc.Id, inc);
        Debug.WriteLine("Income Updated online");
    }

    async Task DeleteIncomeOnlineAsync(IncomeModel inc)
    {
        await AllIncomesOnline?.ReplaceOneAsync(x => x.Id == inc.Id, inc);
    }
    //think of adding a method to undelete an income

    public Task DropIncomesCollection()
    {
        throw new NotImplementedException();
    }
}
