using ZstdSharp.Unsafe;

namespace FlowHub.DataAccess.Repositories;

public class DebtRepository : IDebtRepository
{
    private const string DebtsCollectionName = "DebtsCollection";
    LiteDatabaseAsync db;
    private ILiteCollectionAsync<DebtModel> AllDebts;
    IMongoDatabase DBOnline;
    private IMongoCollection<DebtModel> AllDebtsOnline;
    private readonly IDataAccessRepo dataAccess;
    private readonly IOnlineCredentialsRepository onlineRepository;
    private readonly IUsersRepository usersRepo;

    public List<DebtModel> OfflineDebtList { get; set; }
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
            await OpenDB();
            
            string userId = usersRepo.OfflineUser.Id;
            string userCurrency = usersRepo.OfflineUser.UserCurrency;
            if (usersRepo.OfflineUser.UserIDOnline != string.Empty)
            {
                userId = usersRepo.OfflineUser.UserIDOnline;
            }
            //await AllDebts.DeleteAllAsync();
            
            OfflineDebtList = await AllDebts.Query().ToListAsync();
            var ss = OfflineDebtList
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.UpdateDateTime);

            //OfflineDebtList ??= Enumerable.Empty<DebtModel>().ToList();
            return OfflineDebtList;
            
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.InnerException.Message);
            Debug.WriteLine("Get all Debts fxn Exception: "+ex.Message);
            return Enumerable.Empty<DebtModel>().ToList();
        }
        finally 
        { 
            db.Dispose();
            
        }
    }

    async Task LoadOnlineDB()
    {
        try
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
            var filtersDebt = Builders<DebtModel>.Filter.Eq("UserId", usersRepo.OnlineUser.Id) &
                Builders<DebtModel>.Filter.Eq("Currency", usersRepo.OfflineUser.UserCurrency);

            AllDebtsOnline ??= DBOnline.GetCollection<DebtModel>("Debts");
            OnlineDebtList = await AllDebtsOnline.Find(filtersDebt).ToListAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ERROR WHEN LOADING ONLINE DB " + ex.Message + " " + ex.InnerException.Message);
        }
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

            foreach (var itemID in OfflineDebtDict.Keys.Intersect(OnlineDebtDict.Keys))
            {
                var offlineItem = OfflineDebtDict[itemID];
                var onlineItem = OnlineDebtDict[itemID];
                if (offlineItem.UpdateDateTime.ToUniversalTime() > onlineItem.UpdateDateTime.ToUniversalTime())
                {
                    await UpdateDebtOnlineAsync(offlineItem);
                }
                else if (offlineItem.UpdateDateTime.ToUniversalTime() < onlineItem.UpdateDateTime.ToUniversalTime())
                {
                    await UpdateDebtAsync(onlineItem);
                }
            }

            foreach (var itemID in OfflineDebtDict.Keys.Except(OnlineDebtDict.Keys))
            {
                try
                {
                    await AddDebtOnlineAsync(OfflineDebtDict[itemID]);
                    OnlineDebtList.Add(OfflineDebtDict[itemID]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            foreach (var itemID in OnlineDebtDict.Keys.Except(OfflineDebtDict.Keys))
            {
                try
                {
                    await AddDebtAsync(OnlineDebtDict[itemID]);
                    OfflineDebtList.Add(OnlineDebtDict[itemID]);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Debts Sync exception" + ex.Message);
        }
        finally
        {
            IsBatchUpdate = false;
            OfflineDebtListChanged?.Invoke();
            IsSyncing = false;
            await usersRepo.UpdateUserOnlineGetSetLatestValues(usersRepo.OnlineUser);

        }
    }
    public async Task<bool> AddDebtAsync(DebtModel debt)
    {
        debt.UpdateDateTime = DateTime.UtcNow;
        try
        {
            await OpenDB();
            
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
                    Debug.WriteLine("not notified");
                }
                db.Dispose();
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to add local debt");
                return false;
            }
            
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to add local debt: " + ex.Message);
            db.Dispose();
            return false;
        }
    }
    public async Task<bool> UpdateDebtAsync(DebtModel debt)
    {
        debt.UpdateDateTime = DateTime.UtcNow;

        try
        {
            await OpenDB();
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
                db.Dispose();
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to update local debt");
                return false;
            }
            
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Exception when doing local dept update " + ex.InnerException.Message);
            return false;
        }
    }
    public async Task<bool> DeleteDebtAsync(DebtModel debt)
    {
        debt.UpdateDateTime = DateTime.UtcNow;
        debt.IsDeleted = true;
        try
        {
            await OpenDB();
            
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
                db.Dispose();
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to delete local debt");
                db.Dispose();
                return false;
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
        Debug.WriteLine("Updated online debt");
    }
    async Task DeleteDebtOnlineAsync(DebtModel debtItem)
    {
        await AllDebtsOnline.ReplaceOneAsync(debt => debt.Id == debtItem.Id, debtItem);
    }
    public async Task DropDebtCollection()
    {
        await OpenDB();
        await db.DropCollectionAsync(DebtsCollectionName);
        db.Dispose();
        Debug.WriteLine("debts Collection dropped!");
    }
}