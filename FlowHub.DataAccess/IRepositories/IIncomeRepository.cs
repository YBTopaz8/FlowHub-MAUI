
using FlowHub.Models;
using LiteDB;

namespace FlowHub.DataAccess.IRepositories;

public interface IIncomeRepository
{
    Task<List<IncomeModel>> GetAllIncomesAsync();
    List<IncomeModel> OfflineIncomesList { get; set ; }
    Task<bool> AddIncomeAsync(IncomeModel income);
    Task<bool> DeleteIncomeAsync(BsonValue incomeId);
    Task<bool> UpdateIncomeAsync(IncomeModel income);
    Task DropIncomesCollection();

}
