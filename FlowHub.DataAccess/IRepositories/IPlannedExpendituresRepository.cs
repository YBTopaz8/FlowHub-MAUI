using FlowHub.Models;

namespace FlowHub.DataAccess.IRepositories;

public interface IPlannedExpendituresRepository
{
    Task<List<PlannedExpendituresModel>> GetAllPlannedExp();

    List<PlannedExpendituresModel> OfflinePlannedExpendituresList { get; set; }
    Task<bool> AddPlannedExp(PlannedExpendituresModel plannedExpendituresModel);
    Task<bool> UpdatePlannedExp(PlannedExpendituresModel plannedExpendituresModel);
    Task<bool> DeletePlannedExp(string id);
    Task<bool> SynchronizePlannedExpendituresAsync(string userEmail, string userPassword);

    Task DropPlannedExpCollection();
}
