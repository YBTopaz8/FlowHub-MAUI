
namespace FlowHub.DataAccess.Repositories;

public class ExpendituresRepository : IExpendituresRepository
{
    LiteDatabaseAsync db;
    IMongoDatabase DBOnline;
    List<ExpendituresModel> OnlineExpendituresList { get; set; }

    public List<ExpendituresModel> OfflineExpendituresList { get; set; }

    bool IsBatchUpdate;

    public event Action OfflineExpendituresListChanged;

    private IMongoCollection<ExpendituresModel> AllOnlineExpenditures;

    ILiteCollectionAsync<ExpendituresModel> AllExpenditures;

    private readonly IDataAccessRepo dataAccessRepo;
    private readonly IUsersRepository usersRepo;
    private readonly IOnlineCredentialsRepository onlineDataAccessRepo;

    private const string expendituresDataCollectionName = "Expenditures";

    public ExpendituresRepository(IDataAccessRepo dataAccess, IOnlineCredentialsRepository onlineRepository, IUsersRepository userRepo)
    {
        dataAccessRepo = dataAccess;
        usersRepo = userRepo;
        onlineDataAccessRepo = onlineRepository;
    }

    async Task<LiteDatabaseAsync> OpenDB()
    {
        db = dataAccessRepo.GetDb();

        AllExpenditures = db.GetCollection<ExpendituresModel>(expendituresDataCollectionName);
        await AllExpenditures.EnsureIndexAsync(x => x.Id);
        return db;
    }

    public async Task<List<ExpendituresModel>> GetAllExpendituresAsync()
    {
        if (OfflineExpendituresList is not null)
        {
            return OfflineExpendituresList;
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
            OfflineExpendituresList = await AllExpenditures.Query()
            .Where(x => x.UserId == userId && x.Currency == userCurrency).ToListAsync();

            db.Dispose();
            OfflineExpendituresList ??= Enumerable.Empty<ExpendituresModel>().ToList();
            return OfflineExpendituresList;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.InnerException.Message);
            Debug.WriteLine("Get all EXP fxn Exception: " + ex.Message);
            return Enumerable.Empty<ExpendituresModel>().ToList();
        }
    }

    async Task LoadOnlineDB()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            return;
        }
        if (OnlineExpendituresList is not null)
        {
            return;
        }
        if (DBOnline is null)
        {
            onlineDataAccessRepo.GetOnlineConnection();
            DBOnline = onlineDataAccessRepo.OnlineMongoDatabase;
        }

        if (usersRepo.OnlineUser is null)
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

        FilterDefinition<ExpendituresModel> filtersExpenditures = Builders<ExpendituresModel>.Filter.Eq("UserId", usersRepo.OnlineUser.Id) &
            Builders<ExpendituresModel>.Filter.Eq("Currency", usersRepo.OfflineUser.UserCurrency);

        AllOnlineExpenditures = DBOnline.GetCollection<ExpendituresModel>(expendituresDataCollectionName);
        //try 
        //{ CODE TO REMOVE A FIELD FROM ONLINE
        //    var updateDef = Builders<ExpendituresModel>.Update.Unset("UpdateOnSync");
        //    var filterExp = Builders<ExpendituresModel>.Filter.Empty;
        //    await AllOnlineExpenditures.UpdateManyAsync(filterExp, updateDef);
        //}
        //catch (Exception ex)
        //{
        //    Debug.WriteLine("Exception : " + ex.Message);
        //}
        OnlineExpendituresList = await AllOnlineExpenditures.Find(filtersExpenditures).ToListAsync();
    }

    bool IsSyncing;

    public async Task SynchronizeExpendituresAsync()
    {
        await GetAllExpendituresAsync();
        
        if (!Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
        {
            IsSyncing = false;
            IsBatchUpdate = false;
            OfflineExpendituresListChanged?.Invoke();
            return;
        }
        try
        {
            await LoadOnlineDB();
            if (usersRepo.OnlineUser is null)
            {
                return;
            }
            if (OnlineExpendituresList.Count < 1 && OfflineExpendituresList.Count < 1)
            {
                return;
            }
            IsSyncing = true;
            IsBatchUpdate = true;

            OfflineExpendituresList = new List<ExpendituresModel>(OnlineExpendituresList);
            
            OnlineExpendituresList.Clear();
            return;
            Dictionary<string, ExpendituresModel> OnlineExpendituresDict = OnlineExpendituresList.ToDictionary(x => x.Id, x => x);
            Dictionary<string, ExpendituresModel> OfflineExpendituresDict = OfflineExpendituresList.ToDictionary(x => x.Id, x => x);

            foreach (var itemID in OfflineExpendituresDict.Keys.Intersect(OnlineExpendituresDict.Keys))
            {
                var offlineItem = OfflineExpendituresDict[itemID];
                var onlineItem = OnlineExpendituresDict[itemID];

                if (offlineItem.UpdatedDateTime.ToUniversalTime() > onlineItem.UpdatedDateTime.ToUniversalTime())
                {
                    await UpdateExpenditureOnlineAsync(offlineItem);
                }
                else if (offlineItem.UpdatedDateTime.ToUniversalTime() < onlineItem.UpdatedDateTime.ToUniversalTime())
                {
                    await UpdateExpenditureAsync(onlineItem);
                }
            }

            foreach (var itemID in OfflineExpendituresDict.Keys.Except(OnlineExpendituresDict.Keys))
            {
                await AddExpenditureOnlineAsync(OfflineExpendituresDict[itemID]);
                OnlineExpendituresList.Add(OfflineExpendituresDict[itemID]);
            }

            foreach (var itemID in OnlineExpendituresDict.Keys.Except(OfflineExpendituresDict.Keys))
            {
                await AddExpenditureAsync(OnlineExpendituresDict[itemID]);
            }

        }
        catch (Exception ex)
        {
            await db.RollbackAsync();
            Debug.WriteLine("Expenditures Sync Exception Message : " + ex.Message);
        }
        finally
        {
            await usersRepo.UpdateUserOnlineGetSetLatestValues(usersRepo.OfflineUser);
            IsSyncing = false;
            IsBatchUpdate = false;
            OfflineExpendituresListChanged?.Invoke();

        }

        
    }
    /*--------- SECTION FOR OFFLINE CRUD OPERATIONS----------*/
    public async Task<bool> AddExpenditureAsync(ExpendituresModel expenditure)
    {
        try
        {
            await OpenDB();

            if (await AllExpenditures.InsertAsync(expenditure) is not null)
            {
                OfflineExpendituresList.Add(expenditure);
                if (!IsBatchUpdate)
                {
                    OfflineExpendituresListChanged?.Invoke();
                }

                if (!IsSyncing && Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    OnlineExpendituresList.Add(expenditure);
                    await AddExpenditureOnlineAsync(expenditure);
                }
                //db.Dispose();
                return true;
            }
            else
            {
                Debug.WriteLine("Error while inserting Expenditure");
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Add ExpLocal " + ex.InnerException.Message);
            return false;
        }
    }

    public async Task<bool> UpdateExpenditureAsync(ExpendituresModel expenditure)
    {
        try
        {
            await OpenDB();

            if (await AllExpenditures.UpdateAsync(expenditure))
            {
                Debug.WriteLine("Expenditure updated locally");

                int index = OfflineExpendituresList.FindIndex(x => x.Id == expenditure.Id);
                OfflineExpendituresList[index] = expenditure;
                if (!IsBatchUpdate)
                {
                    OfflineExpendituresListChanged?.Invoke();
                }
                if (!IsSyncing && Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    await UpdateExpenditureOnlineAsync(expenditure);
                }
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to update Expenditure");
                return false;
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return false;
        }
        finally
        {
            db.Dispose();
        }
    }

    public async Task<bool> DeleteExpenditureAsync(ExpendituresModel expenditure)
    {
        expenditure.IsDeleted = true;
        try
        {
            await OpenDB();
            
            if (await AllExpenditures.UpdateAsync(expenditure))
            {
                OfflineExpendituresList.Remove(expenditure);
                Debug.WriteLine("Expenditure deleted locally");
                if (!IsBatchUpdate)
                {
                    OfflineExpendituresListChanged?.Invoke();
                }
                if (!IsSyncing && Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    await DeleteExpenditureOnlineAsync(expenditure);
                }
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to delete Expenditure");
                return false;
            }
            
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return false;
        }
    }

    /*--------- SECTION FOR ONLINE CRUD OPERATIONS OF SINGLE DOCUMENTS----------*/
    private async Task AddExpenditureOnlineAsync(ExpendituresModel expenditure)
    {
        await AllOnlineExpenditures.InsertOneAsync(expenditure);
        Debug.WriteLine("Expenditure added online");
    }

    private async Task UpdateExpenditureOnlineAsync(ExpendituresModel exp)
    {
        await AllOnlineExpenditures?.ReplaceOneAsync(x => x.Id == exp.Id, exp);
    }

    private async Task DeleteExpenditureOnlineAsync(ExpendituresModel exp)
    {
        await AllOnlineExpenditures?.ReplaceOneAsync(x => x.Id == exp.Id, exp);
    }

    public async Task DropExpendituresCollection()
    {
        await OpenDB();
        await db.DropCollectionAsync(expendituresDataCollectionName);
        db.Dispose();
        Debug.WriteLine("Expenditures Collection dropped!");
    }
}
