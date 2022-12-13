using FlowHub.Models;

namespace FlowHub.DataAccess.IRepositories;

public interface IPlannedExpendituresRepository
{
    Task<List<PlannedExpendituresModel>> GetAllMonthlyPlannedExp();

    Task<bool> AddMonthlyPlannedExp(PlannedExpendituresModel plannedExpendituresModel); 
    Task<bool> UpdateMonthlyPlannedExp(PlannedExpendituresModel plannedExpendituresModel); 
    Task<bool> DeleteMonthlyPlannedExp(PlannedExpendituresModel plannedExpendituresModel);

    Task DropMonthlyPlannedExpCollection();
}
