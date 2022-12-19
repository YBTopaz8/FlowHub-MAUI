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
    public List<ExpendituresModel> OfflineExpendituresList { get; set; }    

    ILiteCollectionAsync<ExpendituresModel> AllExpenditures;

    private readonly IDataAccessRepo dataAccessRepo;
    private readonly IUsersRepository usersRepo;

    private const string expendituresDataCollectionName = "Expenditures";   

    public ExpendituresRepository(IDataAccessRepo dataAccess, IUsersRepository userRepo)
    {
        dataAccessRepo = dataAccess;
        usersRepo = userRepo;
    }

    void OpenDB()
    {
        db = dataAccessRepo.GetDb();
        AllExpenditures = db.GetCollection<ExpendituresModel>(expendituresDataCollectionName);     
    }


    public async Task<List<ExpendituresModel>> GetAllExpendituresAsync(string userId)
    {        
        OpenDB();
        OfflineExpendituresList = await AllExpenditures.Query().Where(x => x.UserId == userId).ToListAsync();
        db.Dispose();
        return OfflineExpendituresList;
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
            Debug.WriteLine(ex.Message);
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

    /*--------- SECTION FOR ONLINE CRUD OPERATIONS OF SINGLE DOCUMENTS----------*/
    //async Task AddExpenditureOnlineAsync(ExpendituresModel expenditure)
    //{
    //   // await onlineExpendituresCollection.InsertOneAsync(expenditure);
    //}
    //async Task UpdateExpenditureOnlineAsync(ExpendituresModel exp)
    //{
    // //   await onlineExpendituresCollection.ReplaceOneAsync(x => x.Id == exp.Id, exp);
    //}
    //async Task DeleteExpenditureOnlineAsync(ExpendituresModel exp)
    //{
    //  //  await onlineExpendituresCollection.DeleteOneAsync(x => x.Id == exp.Id);
    //}


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
