namespace FlowHub.DataAccess.Repositories;

public class PlannedExpendituresRepository : IPlannedExpendituresRepository
{
    LiteDatabaseAsync db;
    IMongoDatabase DBOnline;

    ILiteCollectionAsync<PlannedExpendituresModel> AllPlannedExpenditures;
    ILiteCollectionAsync<IDsToBeDeleted> AllIDsToBeDeleted;

    IMongoCollection<PlannedExpendituresModel> AllOnlinePlannedExp;
    private IMongoCollection<IDsToBeDeleted> AllOnlineIDsToBeDeleted;

    List<PlannedExpendituresModel> OnlinePlannedExpList { get; set; }
    public List<PlannedExpendituresModel> OfflinePlannedExpendituresList { get; set; }

    private readonly IDataAccessRepo dataAccessRepo;
    private readonly IUsersRepository usersRepo;
    private readonly IOnlineCredentialsRepository onlineDataAccessRepo;

    private const string AllPlannedExpendituresDataCollectionName = "PlannedExpenditures";
    private const string IDsDataCollectionName = "IDsToDelete";

    public PlannedExpendituresRepository(IDataAccessRepo dataAccess, IUsersRepository userRepo, IOnlineCredentialsRepository onlineRepository)
    {
        dataAccessRepo = dataAccess;
        usersRepo = userRepo;
        onlineDataAccessRepo = onlineRepository;
    }

    void OpenDB()
    {
        db = dataAccessRepo.GetDb();
        AllPlannedExpenditures = db.GetCollection<PlannedExpendituresModel>(AllPlannedExpendituresDataCollectionName);
        AllIDsToBeDeleted = db.GetCollection<IDsToBeDeleted>(IDsDataCollectionName);
    }

    public async Task<List<PlannedExpendituresModel>> GetAllPlannedExp()
    {
        OpenDB();
        OfflinePlannedExpendituresList = await AllPlannedExpenditures.Query().ToListAsync();
        db.Dispose();
        return OfflinePlannedExpendituresList;
    }

    public async Task<bool> AddPlannedExp(PlannedExpendituresModel plannedExpendituresModel)
    {
        plannedExpendituresModel.PlatformModel = DeviceInfo.Current.Model;
        try
        {
            OpenDB();
            if (await AllPlannedExpenditures.InsertAsync(plannedExpendituresModel) is not null)
            {
                await AllPlannedExpenditures.EnsureIndexAsync(x => x.Id);
                db.Dispose();
                return true;
            }
        }
        catch (Exception ex)
        {
            db.Dispose();
            Debug.WriteLine(ex.Message);
        }
        return false;
    }
    public async Task<bool> UpdatePlannedExp(PlannedExpendituresModel plannedExpendituresModel)
    {
        plannedExpendituresModel.PlatformModel = DeviceInfo.Current.Model;
        try
        {
            OpenDB();
            if (await AllPlannedExpenditures.UpdateAsync(plannedExpendituresModel))
            {
                db.Dispose();
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to update monthly planned Expenditure");
                db.Dispose();
                return false;
            }
        }
        catch (Exception ex)
        {
            db.Dispose();
            Debug.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<bool> DeletePlannedExp(string id)
    {
        try
        {
            OpenDB();
            if (await AllPlannedExpenditures.DeleteAsync(id))
            {
                IDsToBeDeleted idToBeDeleted = new()
                {
                    Id = $"PExp_{id}",
                    UserID = usersRepo.OfflineUser.UserIDOnline ?? usersRepo.OfflineUser.Id,
                    PlatformModel = DeviceInfo.Current.Model
                };

                await AllIDsToBeDeleted.InsertAsync(idToBeDeleted);
                db.Dispose();
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to delete planned Exp");
                db.Dispose();
                return false;
            }
        }
        catch (Exception ex)
        {
            db.Dispose();
            Debug.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<bool> SynchronizePlannedExpendituresAsync(string userEmail, string userPassword)
    {
        var filterUserCredentials = Builders<UsersModel>.Filter.Eq("Email", userEmail) &
                                    Builders<UsersModel>.Filter.Eq("Password", userPassword);

        if (DBOnline is null)
        {
            onlineDataAccessRepo.GetOnlineConnection();
            DBOnline = onlineDataAccessRepo.OnlineMongoDatabase;
        }

        if (usersRepo.OnlineUser is null)
        {
            usersRepo.OnlineUser = await DBOnline.GetCollection<UsersModel>("Users").Find(filterUserCredentials).FirstOrDefaultAsync();

            if (usersRepo.OnlineUser is null)
            {
                Debug.WriteLine("Online User not found");
                return false;
            }
            else
            {
                usersRepo.OfflineUser.UserIDOnline = usersRepo.OnlineUser.Id;
            }
        }

        var filter = Builders<PlannedExpendituresModel>.Filter.Eq("UserId", usersRepo.OfflineUser.UserIDOnline);

        AllOnlinePlannedExp ??= DBOnline?.GetCollection<PlannedExpendituresModel>(AllPlannedExpendituresDataCollectionName);

        OnlinePlannedExpList = await AllOnlinePlannedExp.Find(filter).ToListAsync();
        var tempPlannedExpList = await GetAllPlannedExp();

        if (await DeleteAllPlannedExpOnline() && await DeleteAllPlannedExpOffline())
        {
            if (tempPlannedExpList.Count == 0)
            {
                foreach (var exp in OnlinePlannedExpList)
                {
                    exp.UserId = usersRepo.OfflineUser.UserIDOnline;
                    await AddPlannedExp(exp);
                }
                await GetAllPlannedExp();
                return true;
            }
            else
            {
                await UpdateOnlineDBWithLocalData(tempPlannedExpList);

                await UpdateLocalDBWithOnlineData(tempPlannedExpList);

                await GetAllPlannedExp();

                return true;
            }
        }
        return true;
    }

    private async Task UpdateLocalDBWithOnlineData(List<PlannedExpendituresModel> tempPlannedExpList)
    {
        foreach (var pExpOnline in OnlinePlannedExpList)
        {
            if (tempPlannedExpList.Exists(x => x.Id == pExpOnline.Id) && pExpOnline.UpdateOnSync
                && pExpOnline.PlatformModel != DeviceInfo.Current.Model)
            {
                pExpOnline.PlatformModel = DeviceInfo.Current.Model;
                await UpdatePlannedExp(pExpOnline);
            }
            else if (!tempPlannedExpList.Exists(x => x.Id == pExpOnline.Id))
            {
                if (string.IsNullOrEmpty(pExpOnline.Currency) || pExpOnline.Currency?.Length == 0)
                {
                    pExpOnline.Currency = usersRepo.OnlineUser.UserCurrency;
                    await UpdatePlannedExpOnlineAsync(pExpOnline);
                }
                await AddPlannedExp(pExpOnline);
            }
        }
    }

    private async Task UpdateOnlineDBWithLocalData(List<PlannedExpendituresModel> tempPlannedExpList)
    {
        foreach (var pExpOffline in tempPlannedExpList)
        {
            if (!OnlinePlannedExpList.Exists(x => x.Id == pExpOffline.Id))
            {
                pExpOffline.UserId = usersRepo.OfflineUser.UserIDOnline;
                pExpOffline.PlatformModel = DeviceInfo.Current.Model;
                await AddPlannedExpOnlineAsync(pExpOffline);
            }
            else
            {
                if (pExpOffline.UpdateOnSync && pExpOffline.PlatformModel == DeviceInfo.Current.Model)
                {
                    pExpOffline.PlatformModel = DeviceInfo.Current.Model;

                    await UpdatePlannedExpOnlineAsync(pExpOffline);
                }
            }
        }
    }

    private async Task<bool> DeleteAllPlannedExpOnline()
    {
        OpenDB();
        var AllOfflineIDsToBeDeleted = await AllIDsToBeDeleted.Query().ToListAsync();
        db.Dispose();
        var idsFilter = Builders<IDsToBeDeleted>.Filter.Eq("UserID", usersRepo.OnlineUser.Id);
        AllOnlineIDsToBeDeleted ??= DBOnline?.GetCollection<IDsToBeDeleted>(IDsDataCollectionName);
        foreach (var idToDelete in AllOfflineIDsToBeDeleted)
        {
            if (idToDelete.UserID == usersRepo.OnlineUser.Id)
            {
                string id = idToDelete.Id.Remove(0, 5);
                await DeletePlannedExpOnlineAsync(id);
                _ = OnlinePlannedExpList.RemoveAll(x => x.Id == id);
            }
            try
            {
                await AllOnlineIDsToBeDeleted.InsertOneAsync(idToDelete);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        OpenDB();
        await AllIDsToBeDeleted.DeleteAllAsync();
        db.Dispose();
        return true;
    }
    private async Task<bool> DeleteAllPlannedExpOffline()
    {
        var idsFilter = Builders<IDsToBeDeleted>.Filter.Eq("UserID", usersRepo.OnlineUser.Id);
        var AllOnlineIDsToBeDeleted = await DBOnline?.GetCollection<IDsToBeDeleted>(IDsDataCollectionName)
            .Find(idsFilter).ToListAsync()!;
        foreach (var idToDelete in AllOnlineIDsToBeDeleted)
        {
            if (idToDelete.UserID == usersRepo.OnlineUser.Id && idToDelete.PlatformModel != DeviceInfo.Current.Model)
            {
                string id = idToDelete.Id.Remove(0, 5);
                await DeletePlannedExp(id);
                _ = OfflinePlannedExpendituresList.RemoveAll(x => x.Id == id);
            }
        }
        return true;
    }

    /*--------- SECTION FOR ONLINE CRUD OPERATIONS OF SINGLE DOCUMENTS----------*/
    private async Task AddPlannedExpOnlineAsync(PlannedExpendituresModel pExp)
    {
        try
        {
            await AllOnlinePlannedExp.InsertOneAsync(pExp);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private async Task UpdatePlannedExpOnlineAsync(PlannedExpendituresModel pExp)
    {
        try
        {
            await AllOnlinePlannedExp.ReplaceOneAsync(x => x.Id == pExp.Id, pExp);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private async Task DeletePlannedExpOnlineAsync(string id)
    {
        try
        {
            await AllOnlinePlannedExp.DeleteOneAsync(x => x.Id == id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    public Task DropPlannedExpCollection()
    {
        throw new NotImplementedException();
    }
}
