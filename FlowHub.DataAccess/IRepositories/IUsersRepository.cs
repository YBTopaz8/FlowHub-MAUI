using FlowHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHub.DataAccess.IRepositories;

public interface IUsersRepository
{
    public UsersModel OfflineUser { get; set; }
    public UsersModel OnlineUser { get; set; }
 //   Task<UsersModel> GetUserAsync();
    Task<UsersModel> GetUserAsync(string UserEmail, string UserPassword);
    Task<UsersModel> GetUserAsync(string UserId);
    Task<bool> AddUserAsync(UsersModel user);
    Task<bool> UpdateUserAsync(UsersModel user);
    Task<bool> DeleteUserAsync(UsersModel user);

    Task<bool> CheckIfAnyUserExists();
    Task DropCollection();
}
