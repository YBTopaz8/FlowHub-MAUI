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

    private IMongoCollection<ExpendituresModel> AllOnlineExpenditures;
    ILiteCollectionAsync<ExpendituresModel> AllExpenditures;

    private readonly IDataAccessRepo dataAccessRepo;
    private readonly IUsersRepository usersRepo;
    private readonly IOnlineCredentialsRepository onlineDataAccessRepo;

    private const string expendituresDataCollectionName = "Expenditures";

    private string pathToDeletedIDs =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IDToBeDeleted.txt");

    public ExpendituresRepository(IDataAccessRepo dataAccess, IUsersRepository userRepo, IOnlineCredentialsRepository onlineRepository)
    {
        dataAccessRepo = dataAccess;
        usersRepo = userRepo;
        onlineDataAccessRepo = onlineRepository;
    }

    void OpenDB()
    {
        db = dataAccessRepo.GetDb();
        AllExpenditures = db.GetCollection<ExpendituresModel>(expendituresDataCollectionName);     
    }


    public async Task<List<ExpendituresModel>> GetAllExpendituresAsync()
    {
        try
        {
            OpenDB();
            string userId = usersRepo.OfflineUser.Id;
            if (usersRepo.OfflineUser.UserIDOnline != string.Empty)
            {
                userId = usersRepo.OfflineUser.UserIDOnline;
            }
            OfflineExpendituresList = await AllExpenditures.Query().Where(x => x.UserId == userId).ToListAsync();

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
            if (await AllExpenditures.InsertAsync(expenditure) is not null)
            {
                await AllExpenditures.EnsureIndexAsync(x => x.Id);
                db.Dispose();
                return true;
            }        
            else
            {
                Debug.WriteLine("Error while inserting Expenditure");        
            }
            return true;
        }
        catch (Exception ex)
        {
            db.Dispose();
            Debug.WriteLine(ex.InnerException.Message);
            return false;
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

    public async Task<bool> DeleteExpenditureAsync(ExpendituresModel expenditure)
    {
        try
        {
            OpenDB();
            if (await AllExpenditures.DeleteAsync(expenditure.Id))
            {
                await using StreamWriter textFile = new(pathToDeletedIDs, true);
                await textFile.WriteLineAsync(expenditure.Id);
                db.Dispose();
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
        
        var filter = Builders<ExpendituresModel>.Filter.Eq("UserId", usersRepo.OfflineUser.UserIDOnline);

        AllOnlineExpenditures ??= DBOnline?.GetCollection<ExpendituresModel>(expendituresDataCollectionName);

        OnlineExpendituresList = await DBOnline?.GetCollection<ExpendituresModel>(expendituresDataCollectionName)
            .Find(filter)
            .ToListAsync()!;
        
        if (OfflineExpendituresList.Count == 0)
        {
            OfflineExpendituresList = OnlineExpendituresList;
            foreach (var exp in OfflineExpendituresList)
            {
                exp.UserId = usersRepo.OfflineUser.UserIDOnline;
                exp.Currency = usersRepo.OnlineUser.UserCurrency;
                await AddExpenditureAsync(exp);
            }

            await usersRepo.UpdateUserAsync(usersRepo.OfflineUser);
            return true;
        }
        else
        {
            if (File.Exists(pathToDeletedIDs))
            {
                var listOfDeletedIDs = File.ReadLines(pathToDeletedIDs).ToList();
                foreach (var id in listOfDeletedIDs)
                {
                    await DeleteExpenditureOnlineAsync(id);
                }
                await using (File.Create(pathToDeletedIDs));
            }
            
            foreach (var expOffline in OfflineExpendituresList)
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

            foreach (var expOnline in OnlineExpendituresList)
            {
                if (OfflineExpendituresList.Exists(x => x.Id == expOnline.Id) && expOnline.UpdateOnSync &&
                    expOnline.PlatformModel != DeviceInfo.Current.Model)
                {
                    expOnline.PlatformModel = DeviceInfo.Current.Model;
                    await UpdateExpenditureAsync(expOnline);
                }
                else if (!OfflineExpendituresList.Exists(x => x.Id== expOnline.Id))
                {
                    await AddExpenditureAsync(expOnline);
                }
            }
            
            return true;
        }
        
        
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


    /*--------- SECTION FOR OFFLINE TO ONLINE SYNC OPERATIONS----------*/




    /*---- SECTION FOR ONLINE TO OFFLINE SYNC OPERATIONS ----*/
   

    public async Task DropExpendituresCollection()
    {
        OpenDB();
        await db.DropCollectionAsync(expendituresDataCollectionName);
        db.Dispose();
        Debug.WriteLine("Collection dropped! ");
    }

    //public async Task<List<ExpendituresModel>> GetAllExpFromOnlineAsync(string UserId)
    //{
        
       


    //    //var client = new RestClient("https://data.mongodb-api.com/app/data-czemo/endpoint/data/v1/action/findOne");
    //    //var request = new RestRequest();
    //    //request.AddHeader("Content-Type", "application/json");
    //    //request.AddHeader("Access-Control-Request-Headers", "*");
    //    //request.AddHeader("api-key", $"{onlineCredentials.APIKey}");
    //    //var body = @"{" + "" +@" ""collection"":""Expenditures""," + "" +
    //    //    @" ""database"":""SavingsTracker""," +
    //    //    "" +@" ""dataSource"":""FlowHubCluster"" " +@"}";
    //    //request.AddStringBody(body, DataFormat.Json);
    //    //RestResponse response = await client.PostAsync(request);
    //    //ExpAPIModel = JsonConvert.DeserializeObject<ExpendituresModel>(response.Content);


    //    //var listOfExp = ExpAPIModel.documents;
    //    //return listOfExp;
    //}
}
