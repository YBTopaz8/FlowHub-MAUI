using FlowHub.DataAccess.IRepositories;
using FlowHub.Models;
using LiteDB;
using LiteDB.Async;
using System.Diagnostics;

namespace FlowHub.DataAccess.Repositories;

public class IncomeRepository : IIncomeRepository
{
    LiteDatabaseAsync db;
    public List<IncomeModel> OfflineIncomesList { get; set; }

    ILiteCollectionAsync<IncomeModel> AllIncomes;

    private readonly IDataAccessRepo dataAccessRepo;
    private readonly IUsersRepository usersRepo;

    private const string incomesDataCollectionName = "Incomes";
    public IncomeRepository(IDataAccessRepo dataAccess, IUsersRepository userRepository)
    {
        dataAccessRepo= dataAccess;
        usersRepo= userRepository;
    }

    async Task OpenDB()
    {
        db = dataAccessRepo.GetDb();
        AllIncomes = db.GetCollection<IncomeModel>(incomesDataCollectionName);
        await AllIncomes.EnsureIndexAsync(x => x.Id);
    }

    public async Task<List<IncomeModel>> GetAllIncomesAsync()
    {
        try
        {
            await OpenDB();
            OfflineIncomesList = await AllIncomes.Query().ToListAsync();
            db.Dispose();
            return OfflineIncomesList;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return Enumerable.Empty<IncomeModel>().ToList();
        }
    }
 /*--------- SECTION FOR OFFLINE CRUD OPERATIONS----------*/
    public async Task<bool> AddIncomeAsync(IncomeModel income)
    {
        income.PlatformModel = DeviceInfo.Current.Model;
        await OpenDB();
        try
        {
            if (await AllIncomes.InsertAsync(income) is not null)
            {
                db.Dispose();
                return true;
            }
            else
            {
                Debug.WriteLine("Error while inserting Income");
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

    public async Task<bool> DeleteIncomeAsync(BsonValue incomeId)
    {
        await OpenDB();
        try
        {
            if (await AllIncomes.DeleteAsync(incomeId))
            {
                db.Dispose();
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to delete income");
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

    public async Task<bool> UpdateIncomeAsync(IncomeModel income)
    {
        income.PlatformModel = DeviceInfo.Current.Model;
        try
        {
            await OpenDB();
            if (await AllIncomes.UpdateAsync(income))
            {
                db.Dispose();
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to update income");
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

    public Task DropIncomesCollection()
    {
        throw new NotImplementedException();
    }
}
