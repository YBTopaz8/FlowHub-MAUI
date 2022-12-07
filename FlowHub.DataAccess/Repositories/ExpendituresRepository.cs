using FlowHub.DataAccess.IRepositories;
using LiteDB.Async;
using System.Diagnostics;
using FlowHub.Models;

namespace FlowHub.DataAccess.Repositories;

public class ExpendituresRepository : IExpendituresRepository
{
    LiteDatabaseAsync db;

    public List<ExpendituresModel> OnlineExpList { get; set; }
    public List<ExpendituresModel> ExpList { get; set; }

    ILiteCollectionAsync<ExpendituresModel> AllExpenditures;

    private readonly IDataAccessRepo dataAccessRepo;
    private readonly IUsersRepository usersRepo;
    private readonly IOnlineCredentialsRepository onlineCredentials;

    private const string expendituresDataCollectionName = "Expenditures";
    private UsersModel userOffline = new();
    private UsersModel userOnline = new();

    List<ExpendituresModel> InsertExpList = new();
    List<ExpendituresModel> UpdateExpList = new();
    List<ExpendituresModel> DeleteExpList = new();
    readonly string InsertJSONFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InsertExpData.json");
    readonly string UpdateJSONFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UpdateExpData.json");
    readonly string DeleteJSONFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeleteExpData.json");

    public ExpendituresRepository(IDataAccessRepo dataAccess, IUsersRepository userRepo, IOnlineCredentialsRepository onlineCredentialsRepo)
    {
        dataAccessRepo = dataAccess;
        usersRepo = userRepo;
        onlineCredentials = onlineCredentialsRepo;
    }

    void OpenDB()
    {
        db = dataAccessRepo.GetDb();
        AllExpenditures = db.GetCollection<ExpendituresModel>(expendituresDataCollectionName);
        userOffline ??= usersRepo.OfflineUser; //offline user will be filled when LoginDVM will be called
    }


    public async Task<List<ExpendituresModel>> GetAllExpendituresAsync()
    {        
        OpenDB();
        ExpList = await AllExpenditures.Query().ToListAsync();
        db.Dispose();
        return ExpList;
    }

    /*--------- SECTION FOR OFFLINE CRUD OPERATIONS----------*/
    public async Task<bool> AddExpenditureAsync(ExpendituresModel expenditure)
    {
        expenditure.PlatformModel = DeviceInfo.Current.Model;
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

    public async Task<bool> UpdateExpenditureAsync(ExpendituresModel expenditure)
    {
        expenditure.PlatformModel = DeviceInfo.Current.Model;
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

    public async Task<bool> DeleteExpenditureAsync(ExpendituresModel expenditure)
    {
        OpenDB();
        if (await AllExpenditures.DeleteAsync(expenditure.Id))
        {
            db.Dispose();
            return true;
        }
        else
        {
            Debug.WriteLine("Failed to delete Expenditure");
            db.Dispose();
            throw new Exception("Failed to Delete Expenditure");

        }
    }

    /*--------- SECTION FOR ONLINE CRUD OPERATIONS OF SINGLE DOCUMENTS----------*/
    async Task AddExpenditureOnlineAsync(ExpendituresModel expenditure)
    {
       // await onlineExpendituresCollection.InsertOneAsync(expenditure);
    }
    async Task UpdateExpenditureOnlineAsync(ExpendituresModel exp)
    {
     //   await onlineExpendituresCollection.ReplaceOneAsync(x => x.Id == exp.Id, exp);
    }
    async Task DeleteExpenditureOnlineAsync(ExpendituresModel exp)
    {
      //  await onlineExpendituresCollection.DeleteOneAsync(x => x.Id == exp.Id);
    }


    /*--------- SECTION FOR OFFLINE TO ONLINE SYNC OPERATIONS----------*/
   
  
 

    /*---- SECTION FOR ONLINE TO OFFLINE SYNC OPERATIONS ----*/
    public async Task<bool> AddExpenditureListAsync(List<ExpendituresModel> expenditures)
    {
        OpenDB();
        await AllExpenditures.InsertBulkAsync(expenditures);
        db.Dispose();
        return true;
    }

    public async Task DropExpendituresCollection()
    {
        OpenDB();
        await db.DropCollectionAsync(expendituresDataCollectionName);
        db.Dispose();
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
