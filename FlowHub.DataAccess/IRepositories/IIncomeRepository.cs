namespace FlowHub.DataAccess.IRepositories;

public interface IIncomeRepository
{
    event Action OfflineIncomesListChanged;
    Task<List<IncomeModel>> GetAllIncomesAsync();
    List<IncomeModel> OfflineIncomesList { get; set ; }
    Task<bool> AddIncomeAsync(IncomeModel income);
    Task<bool> DeleteIncomeAsync(IncomeModel incomeId);
    Task<bool> UpdateIncomeAsync(IncomeModel income);
    Task SynchronizeIncomesAsync();
    Task DropIncomesCollection();
}
