using FlowHub.DataAccess.IRepositories;
using LiteDB.Async;
using System.Diagnostics;
using FlowHub.Models;
using MongoDB.Driver;

namespace FlowHub.DataAccess.Repositories;

public class ExpendituresRepository : IExpendituresRepository
{
    LiteDatabaseAsync db;
    IMongoDatabase DBOnline;
    List<ExpendituresModel> OnlineExpendituresList { get; set; }

    public List<ExpendituresModel> OfflineExpendituresList { get; set; }

    bool isBatchUpdate;
    public event Action OfflineExpendituresListChanged;

    private IMongoCollection<ExpendituresModel> AllOnlineExpenditures;
    private IMongoCollection<IDsToBeDeleted> AllOnlineIDsToBeDeleted;

    ILiteCollectionAsync<ExpendituresModel> AllExpenditures;
    ILiteCollectionAsync<IDsToBeDeleted> AllIDsToBeDeleted;

    private readonly IDataAccessRepo dataAccessRepo;
    private readonly IUsersRepository usersRepo;
    private readonly IOnlineCredentialsRepository onlineDataAccessRepo;

    private const string expendituresDataCollectionName = "Expenditures";
    private const string IDsDataCollectionName = "IDsToDelete";

    public ExpendituresRepository(IDataAccessRepo dataAccess, IUsersRepository userRepo, IOnlineCredentialsRepository onlineRepository)
    {
        dataAccessRepo = dataAccess;
        usersRepo = userRepo;
        onlineDataAccessRepo = onlineRepository;
    }

    async void OpenDB()
    {
        db = dataAccessRepo.GetDb();
        AllExpenditures = db.GetCollection<ExpendituresModel>(expendituresDataCollectionName);
        AllIDsToBeDeleted = db.GetCollection<IDsToBeDeleted>(IDsDataCollectionName);

       await AllExpenditures.EnsureIndexAsync(x => x.Id);
    }

    public async Task<List<ExpendituresModel>> GetAllExpendituresAsync()
    {
        try
        {
            OpenDB();
            string userId = usersRepo.OfflineUser.Id;
            string userCurrency = usersRepo.OfflineUser.UserCurrency;
            if (usersRepo.OfflineUser.UserIDOnline != string.Empty)
            {
                userId = usersRepo.OfflineUser.UserIDOnline;
            }
            OfflineExpendituresList = await AllExpenditures.Query().Where(x => x.UserId == userId && x.Currency == userCurrency).ToListAsync();

            db.Dispose();

            return OfflineExpendituresList;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return Enumerable.Empty<ExpendituresModel>().ToList();
        }
    }

    /*--------- SECTION FOR OFFLINE CRUD OPERATIONS----------*/
    public async Task<bool> AddExpenditureAsync(ExpendituresModel expenditure)
    {
        expenditure.PlatformModel = DeviceInfo.Current.Model;

        try
        {
            OpenDB();

            if (!await AllExpenditures.ExistsAsync(x => x.Id == expenditure.Id))
            {
                if (await AllExpenditures.InsertAsync(expenditure) is not null)
                {
                    OfflineExpendituresList.Add(expenditure);
                    if (!isBatchUpdate)
                    {
                        OfflineExpendituresListChanged?.Invoke();
                    }
                    return true;                    
                }
                else
                {
                    Debug.WriteLine("Error while inserting Expenditure");
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Add ExpLocal "+ ex.InnerException.Message);
            return false;
        }
        finally
        {
            db.Dispose();
        }
    }

    public async Task<bool> UpdateExpenditureAsync(ExpendituresModel expenditure)
    {
        expenditure.PlatformModel = DeviceInfo.Current.Model;
        try
        {
            OpenDB();
            if (await AllExpenditures.UpdateAsync(expenditure))
            {
                db.Dispose();
                int index = OfflineExpendituresList.FindIndex(x => x.Id == expenditure.Id);
                OfflineExpendituresList[index] = expenditure;
                if (!isBatchUpdate)
                {
                    OfflineExpendituresListChanged?.Invoke();
                }
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to update Expenditure");
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

    public async Task<bool> DeleteExpenditureAsync(string id)
    {
        try
        {
            OpenDB();
            if (await AllExpenditures.DeleteAsync(id))
            {
                IDsToBeDeleted idToBeDeleted = new()
                {
                    Id = $"Exp_{id}",
                    UserID = usersRepo.OfflineUser.UserIDOnline ?? usersRepo.OfflineUser.Id,
                    PlatformModel = DeviceInfo.Current.Model
                };

                await AllIDsToBeDeleted.InsertAsync(idToBeDeleted);
                db.Dispose();
                OfflineExpendituresList.Remove(OfflineExpendituresList.Where(x => x.Id == id).FirstOrDefault());
                if (!isBatchUpdate)
                {
                    OfflineExpendituresListChanged?.Invoke();
                }
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to delete Expenditure");
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

    public async Task<bool> SynchronizeExpendituresAsync(string userEmail, string userPassword)
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
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception Message {ex.Message}");
            }
        }

        var filter = Builders<ExpendituresModel>.Filter.Eq("UserId", usersRepo.OnlineUser.Id);

        AllOnlineExpenditures ??= DBOnline?.GetCollection<ExpendituresModel>(expendituresDataCollectionName);

        OnlineExpendituresList = await AllOnlineExpenditures.Find(filter).ToListAsync()!;
        var tempExpList = await GetAllExpendituresAsync();

        if (await DeleteAllExpOnline() && await DeleteAllExpOffline())
        {
            if (tempExpList.Count == 0)
            {
                //tempExpList = OnlineExpendituresList;
                foreach (var exp in OnlineExpendituresList)
                {
                    exp.UserId = usersRepo.OfflineUser.UserIDOnline;

                    await AddExpenditureAsync(exp);
                }
                await GetAllExpendituresAsync();

                await usersRepo.UpdateUserAsync(usersRepo.OfflineUser);
                return true;
            }
            else
            {
                await UpdateOnlineDBWithLocalData(tempExpList);

                await UpdateLocalDBWithOnlineData(tempExpList);

                await GetAllExpendituresAsync();
                await usersRepo.UpdateUserOnlineGetSetLatestValues(usersRepo.OfflineUser);

                return true;
            }
        }

        return true;
    }

    private async Task UpdateLocalDBWithOnlineData(List<ExpendituresModel> tempExpList)
    {
        isBatchUpdate = true;
        foreach (var expOnline in OnlineExpendituresList)
        {
            if (tempExpList.Exists(x => x.Id == expOnline.Id) && expOnline.UpdateOnSync &&
                expOnline.PlatformModel != DeviceInfo.Current.Model)
            {
                expOnline.PlatformModel = DeviceInfo.Current.Model;
                await UpdateExpenditureAsync(expOnline);
            }
            else if (!tempExpList.Exists(x => x.Id == expOnline.Id))
            {
                if (string.IsNullOrEmpty(expOnline.Currency) || expOnline.Currency?.Length == 0)
                {
                    expOnline.Currency = usersRepo.OnlineUser.UserCurrency;
                    await UpdateExpenditureOnlineAsync(expOnline);
                }
                await AddExpenditureAsync(expOnline);
            }
        }
        isBatchUpdate = false;
        OfflineExpendituresListChanged?.Invoke();
    }

    private async Task UpdateOnlineDBWithLocalData(List<ExpendituresModel> tempExpList)
    {
        foreach (var expOffline in tempExpList)
        {
            if (!OnlineExpendituresList.Exists(x => x.Id == expOffline.Id))
            {
                expOffline.UserId = usersRepo.OfflineUser.UserIDOnline;
                expOffline.PlatformModel = DeviceInfo.Current.Model;
                await AddExpenditureOnlineAsync(expOffline);
            }
            else
            {
                if (expOffline.UpdateOnSync && expOffline.PlatformModel == DeviceInfo.Current.Model)
                {
                    expOffline.PlatformModel = DeviceInfo.Current.Model;

                    await UpdateExpenditureOnlineAsync(expOffline);
                }
            }
        }
    }

    private async Task<bool> DeleteAllExpOnline()
    {
        OpenDB();
        var AllOfflineIDsToBeDeleted = await AllIDsToBeDeleted.Query().ToListAsync();
        db.Dispose();
        var idsFilter = Builders<IDsToBeDeleted>.Filter.Eq("UserID", usersRepo.OnlineUser.Id);
        AllOnlineIDsToBeDeleted ??=  DBOnline?.GetCollection<IDsToBeDeleted>(IDsDataCollectionName);
        foreach (var idToDelete in AllOfflineIDsToBeDeleted)
        {
            if (idToDelete.UserID == usersRepo.OnlineUser.Id)
            {
                string id = idToDelete.Id.Remove(0, 4);
                await DeleteExpenditureOnlineAsync(id);
                _ = OnlineExpendituresList.RemoveAll(x => x.Id == id);
            }
            try
            {
                await AllOnlineIDsToBeDeleted.InsertOneAsync(idToDelete);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("deleteOnline Ex msg :" + ex.Message);
            }
        }
        OpenDB();
            await AllIDsToBeDeleted.DeleteAllAsync();
        db.Dispose();
        return true;
    }
    private async Task<bool> DeleteAllExpOffline()
    {
        var idsFilter = Builders<IDsToBeDeleted>.Filter.Eq("UserID", usersRepo.OnlineUser.Id);
        var AllOnlineIDsToBeDeleted = await DBOnline?.GetCollection<IDsToBeDeleted>(IDsDataCollectionName)
            .Find(idsFilter).ToListAsync()!;
        foreach (var idToDelete in AllOnlineIDsToBeDeleted)
        {
            if (idToDelete.UserID == usersRepo.OnlineUser.Id && idToDelete.PlatformModel != DeviceInfo.Current.Model)
            {
                string id = idToDelete.Id.Remove(0, 4);
                await DeleteExpenditureAsync(id);
                _ = OfflineExpendituresList.RemoveAll(x => x.Id == id);
            }
        }
        return true;
    }

    /*--------- SECTION FOR ONLINE CRUD OPERATIONS OF SINGLE DOCUMENTS----------*/
    private async Task AddExpenditureOnlineAsync(ExpendituresModel expenditure)
    {
        try
        {
            await AllOnlineExpenditures.InsertOneAsync(expenditure);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private async Task UpdateExpenditureOnlineAsync(ExpendituresModel exp)
    {
        await AllOnlineExpenditures.ReplaceOneAsync(x => x.Id == exp.Id, exp);
    }

    private async Task DeleteExpenditureOnlineAsync(string id)
    {
        await AllOnlineExpenditures.DeleteOneAsync(x => x.Id == id);
    }

    public async Task DropExpendituresCollection()
    {
        OpenDB();
        await db.DropCollectionAsync(expendituresDataCollectionName);
        db.Dispose();
        Debug.WriteLine("Expenditures Collection dropped!");
    }

    public async Task DropCollectionIDsToDelete()
    {
        OpenDB();
        await db.DropCollectionAsync(IDsDataCollectionName);
        db.Dispose();
        Debug.WriteLine("IDs Collection dropped!");
        //Display alert saying that the collection was dropped
        await Shell.Current.DisplayAlert("Alert", "Collection dropped!", "OK");
    }
}
