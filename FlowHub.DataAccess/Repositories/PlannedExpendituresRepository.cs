using FlowHub.DataAccess.IRepositories;
using FlowHub.Models;
using LiteDB.Async;
using System.Diagnostics;

namespace FlowHub.DataAccess.Repositories;

public class PlannedExpendituresRepository : IPlannedExpendituresRepository
{
    LiteDatabaseAsync db;

    ILiteCollectionAsync<PlannedExpendituresModel> AllMonthlyPlannedExpenditures;

    List<PlannedExpendituresModel> OfflineMonthlyPlannedExp { get; set; }

    private readonly IDataAccessRepo dataAccessRepo;
    private readonly IUsersRepository usersRepo;

    private const string AllMonthlyPlannedExpendituresDataCollectionName = "MonthlyPlannedExpenditures";
    public PlannedExpendituresRepository(IDataAccessRepo dataAccess, IUsersRepository userRepo)
    {
        dataAccessRepo = dataAccess;
        usersRepo = userRepo;
    }

    void OpenDBMonthlyPlanned()
    {
        db = dataAccessRepo.GetDb();
        AllMonthlyPlannedExpenditures = db.GetCollection<PlannedExpendituresModel>(AllMonthlyPlannedExpendituresDataCollectionName);
    }

    public async Task<List<PlannedExpendituresModel>> GetAllMonthlyPlannedExp()
    {
        OpenDBMonthlyPlanned();
        OfflineMonthlyPlannedExp = await AllMonthlyPlannedExpenditures.Query().ToListAsync();
        db.Dispose();
        return OfflineMonthlyPlannedExp.FindAll(x => x.IsMonthlyPlanned == true);
    }

    public async Task<bool> AddMonthlyPlannedExp(PlannedExpendituresModel plannedExpendituresModel)
    {
        plannedExpendituresModel.PlatformModel = DeviceInfo.Current.Model;
        try
        {
            OpenDBMonthlyPlanned();
            if (await AllMonthlyPlannedExpenditures.InsertAsync(plannedExpendituresModel) is not null)
            {
                await AllMonthlyPlannedExpenditures.EnsureIndexAsync(x => x.Id);
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
    public async Task<bool> UpdateMonthlyPlannedExp(PlannedExpendituresModel plannedExpendituresModel)
    {
        plannedExpendituresModel.PlatformModel = DeviceInfo.Current.Model;
        try
        {
            OpenDBMonthlyPlanned();
            if (await AllMonthlyPlannedExpenditures.UpdateAsync(plannedExpendituresModel))
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

    public async Task<bool> DeleteMonthlyPlannedExp(PlannedExpendituresModel plannedExpendituresModel)
    {
        try
        {
            OpenDBMonthlyPlanned();
            if (await AllMonthlyPlannedExpenditures.DeleteAsync(plannedExpendituresModel.Id))
            {
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

    public Task DropMonthlyPlannedExpCollection()
    {
        throw new NotImplementedException();
    }


}
