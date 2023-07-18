using ZstdSharp.Unsafe;

namespace FlowHub.DataAccess.Repositories;

public class DebtRepository : IDebtRepository
{
    private const string DebtsCollectionName = "Debts";
    LiteDatabaseAsync db;
    private ILiteCollectionAsync<DebtModel> AllDebts;
    IMongoDatabase DBOnline;
    private IMongoCollection<DebtModel> AllDebtsOnline;
    private readonly IDataAccessRepo dataAccess;
    private readonly IOnlineCredentialsRepository onlineRepository;
    private readonly IUsersRepository usersRepo;

    public List<DebtModel> OfflineDebtList { get ; set ; }
    public List<DebtModel> OnlineDebtList { get; set; }

    public event Action OfflineDebtListChanged;

    public DebtRepository(IDataAccessRepo dataAccess, IOnlineCredentialsRepository onlineRepository, IUsersRepository userRepo)
    {
        this.dataAccess = dataAccess;
        this.onlineRepository = onlineRepository;
        usersRepo = userRepo;
    }

    async Task<LiteDatabaseAsync> OpenDB()
    {
        db = dataAccess.GetDb();
        AllDebts = db.GetCollection<DebtModel>(DebtsCollectionName);
        await AllDebts.EnsureIndexAsync(x => x.Id);
        return db;
    }

    public async Task<List<DebtModel>> GetAllDebtAsync()
    {
        try
        {
            using (db = await OpenDB())
            {
                string userId = usersRepo.OfflineUser.Id;
                string userCurrency = usersRepo.OfflineUser.UserCurrency;
                if (usersRepo.OfflineUser.UserIDOnline != string.Empty)
                {
                    userId = usersRepo.OfflineUser.UserIDOnline;
                }
                OfflineDebtList = await AllDebts.Query()
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.UpdateDateTime)
                    .ToListAsync();
                OfflineDebtList ??= Enumerable.Empty<DebtModel>().ToList();
                return OfflineDebtList;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return Enumerable.Empty<DebtModel>().ToList();
        }
    }

    async Task LoadOnlineDB()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            return;
        }
        if (OnlineDebtList is not null)
        {
            return;
        }

        if (DBOnline is null)
        {
            onlineRepository.GetOnlineConnection();
            DBOnline = onlineRepository.OnlineMongoDatabase;
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
        var filtersDebt= Builders<DebtModel>.Filter.Eq("UserId", usersRepo.OnlineUser.Id) &
            Builders<DebtModel>.Filter.Eq("Currency", usersRepo.OfflineUser.UserCurrency);

        AllDebtsOnline ??= DBOnline.GetCollection<DebtModel>(DebtsCollectionName);
        OnlineDebtList = await AllDebtsOnline.Find(filtersDebt).ToListAsync();
    }

    bool IsSyncing;
    private bool IsBatchUpdate;

    public async Task SynchronizeDebtsAsync()
    {
        try
        {
            await GetAllDebtAsync();
            await LoadOnlineDB();
            if (usersRepo.OnlineUser is null)
            {
                return; //no user online
            }

            if (OnlineDebtList.Count < 1 && OfflineDebtList.Count < 1)
            {
                return; //no need to sync
            }
            IsSyncing = true;
            IsBatchUpdate = true;

            Dictionary<string, DebtModel> OnlineDebtDict = OnlineDebtList.ToDictionary(x => x.Id, x => x);
            Dictionary<string, DebtModel> OfflineDebtDict = OfflineDebtList.ToDictionary(x => x.Id, x => x);

            foreach(var itemID in OfflineDebtDict.Keys.Intersect(OnlineDebtDict.Keys))
            {
                var offlineItem = OfflineDebtDict[itemID];
                var onlineItem = OnlineDebtDict[itemID];
                if (offlineItem.UpdateDateTime > onlineItem.UpdateDateTime)
                {
                    await UpdateDebtOnlineAsync(offlineItem);
                }
                else if (offlineItem.UpdateDateTime < onlineItem.UpdateDateTime)
                {
                    await UpdateDebtAsync(onlineItem);
                }
            }

            foreach (var itemID in OfflineDebtDict.Keys.Except(OnlineDebtDict.Keys))
            {
                await AddDebtOnlineAsync(OfflineDebtDict[itemID]);
                OnlineDebtList.Add(OfflineDebtDict[itemID]);
            }
            foreach (var itemID in OnlineDebtDict.Keys.Except(OfflineDebtDict.Keys))
            {
                await AddDebtAsync(OnlineDebtDict[itemID]);
                OfflineDebtList.Add(OnlineDebtDict[itemID]);
            }

            await usersRepo.UpdateUserOnlineGetSetLatestValues(usersRepo.OnlineUser);
            IsSyncing = false;
            IsBatchUpdate = false;
            OfflineDebtListChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Sync exception" + ex.Message);
        }
    }
    public async Task<bool> AddDebtAsync(DebtModel debt)
    {
        try
        {
            using (db = await OpenDB())
            {
                if (await AllDebts.InsertAsync(debt) is not null)
                {
                    OfflineDebtList.Add(debt);
                    Debug.WriteLine("Added local debt");
                    if (!IsBatchUpdate)
                    {
                        OfflineDebtListChanged?.Invoke();
                        Debug.WriteLine("NOTIFIED");
                    }
                    if (!IsSyncing && Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        await AddDebtOnlineAsync(debt);
                    }
                    return true;
                }
                else
                {
                    Debug.WriteLine("Failed to add local debt");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to add local debt: " + ex.Message);
            return false;
        }
    }
    public async Task<bool> UpdateDebtAsync(DebtModel debt)
    {
        try
        {
            using(db = await OpenDB())
            {
                if (await AllDebts.UpdateAsync(debt))
                {
                    Debug.WriteLine("Debt updated locally");

                    int index = OfflineDebtList.FindIndex(x => x.Id == debt.Id);
                    OfflineDebtList[index] = debt;
                    if (!IsBatchUpdate)
                    {
                        OfflineDebtListChanged?.Invoke();
                    }
                    if (!IsSyncing && Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        await UpdateDebtOnlineAsync(debt);
                    }
                    return true;
                }
                else
                {
                    Debug.WriteLine("Failed to update local debt");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Exception when doing local dept update " + ex.Message);
            return false;
        }
    }
    public async Task<bool> DeleteDebtAsync(DebtModel debt)
    {
        debt.IsDeleted = true;
        try
        {
            using(db = await OpenDB())
            {
                if (await AllDebts.UpdateAsync(debt))
                {
                    OfflineDebtList.Remove(debt);
                    Debug.WriteLine("Debt deleted locally");
                    if (!IsBatchUpdate)
                    {
                        OfflineDebtListChanged?.Invoke();
                    }
                    if (!IsSyncing && Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        await DeleteDebtOnlineAsync(debt);
                    }
                    return true;
                }
                else
                {
                    Debug.WriteLine("Failed to delete local debt");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to delete local debt: " + ex.Message);
            return false;
        }
    }

    /* -- ---------------- Online Operations------------------ */
    async Task AddDebtOnlineAsync(DebtModel debtItem)
    {
        await AllDebtsOnline.InsertOneAsync(debtItem);
        Debug.WriteLine("Added online debt");
    }

    async Task UpdateDebtOnlineAsync(DebtModel debtItem)
    {
        await AllDebtsOnline.ReplaceOneAsync(debt => debt.Id == debtItem.Id, debtItem);
    }
    async Task DeleteDebtOnlineAsync(DebtModel debtItem)
    {
        await AllDebtsOnline.ReplaceOneAsync(debt => debt.Id == debtItem.Id, debtItem);
    }
    public Task DropDebtCollection()
    {
        throw new NotImplementedException();
    }

    public Task<bool> SynchronizeDebtAsync(string userEmail, string userPassword)
    {
        throw new NotImplementedException();
    }
}