namespace FlowHub.DataAccess.IRepositories;

public interface IExpendituresRepository
{
    //   Task<List<ExpendituresModel>> GetAllExpendituresAsync();
    event Action OfflineExpendituresListChanged;
    Task<List<ExpendituresModel>> GetAllExpendituresAsync();
   // List<ExpendituresModel> OnlineExpendituresList { get; set; }

    List<ExpendituresModel> OfflineExpendituresList { get; set; }
 //   Task<List<ExpendituresModel>> GetAllExpFromOnlineAsync(string UserId);
    Task<bool> AddExpenditureAsync(ExpendituresModel expenditure);
    Task<bool> UpdateExpenditureAsync(ExpendituresModel expenditure);
    Task<bool> DeleteExpenditureAsync(ExpendituresModel expenditure);

    Task SynchronizeExpendituresAsync();

    Task DropExpendituresCollection();

}
