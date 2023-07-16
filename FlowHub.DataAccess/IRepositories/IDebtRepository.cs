namespace FlowHub.DataAccess.IRepositories;
public interface IDebtRepository
{
    event Action OfflineDebtListChanged;
    Task<List<DebtModel>> GetAllDebtAsync();
    List<DebtModel> OfflineDebtList { get; set; }
    Task<bool> AddDebtAsync(DebtModel debt);
    Task<bool> UpdateDebtAsync(DebtModel debt);
    Task<bool> DeleteDebtAsync(DebtModel debt);
    Task SynchronizeDebtsAsync();
    Task DropDebtCollection();
}
