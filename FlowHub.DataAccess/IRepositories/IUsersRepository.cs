using FlowHub.Models;

namespace FlowHub.DataAccess.IRepositories;

public interface IUsersRepository
{
    event Action OfflineUserDataChanged;
    public UsersModel OfflineUser { get; set; }
    public UsersModel OnlineUser { get; set; }
 //   Task<UsersModel> GetUserAsync();
    Task<UsersModel> GetUserAsync(string UserEmail, string UserPassword);
    Task<UsersModel> GetUserAsync(string UserId);
    Task<UsersModel> GetUserOnlineAsync(UsersModel user);
    Task<bool> AddUserAsync(UsersModel user);
    Task<bool> AddUserOnlineAsync(UsersModel user);
    Task<bool> UpdateUserAsync(UsersModel user);
    Task UpdateUserOnlineAsync(UsersModel user);

    Task<bool> UpdateUserOnlineGetSetLatestValues(UsersModel user);
    Task<bool> DeleteUserAsync(UsersModel user);
    Task<bool> CheckIfAnyUserExists();

    Task DropCollection();
}
