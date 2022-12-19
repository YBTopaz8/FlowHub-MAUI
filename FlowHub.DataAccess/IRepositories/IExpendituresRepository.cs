using FlowHub.Models;

namespace FlowHub.DataAccess.IRepositories;

public interface IExpendituresRepository
{
    //   Task<List<ExpendituresModel>> GetAllExpendituresAsync();
    Task<List<ExpendituresModel>> GetAllExpendituresAsync(string UserID);
    List<ExpendituresModel> OnlineExpList { get; set; }

    List<ExpendituresModel> OfflineExpendituresList { get; set; }
 //   Task<List<ExpendituresModel>> GetAllExpFromOnlineAsync(string UserId);
    Task<bool> AddExpenditureAsync(ExpendituresModel expenditure);
    Task<bool> UpdateExpenditureAsync(ExpendituresModel expenditure);
    Task<bool> DeleteExpenditureAsync(ExpendituresModel expenditure);

    Task DropExpendituresCollection();

}
