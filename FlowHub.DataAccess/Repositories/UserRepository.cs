using FlowHub.DataAccess.IRepositories;
using FlowHub.Models;
using LiteDB.Async;
using System.Diagnostics;

namespace FlowHub.DataAccess.Repositories;

public class UserRepository : IUsersRepository
{

    LiteDatabaseAsync db;
    

    ILiteCollectionAsync<UsersModel> AllUsers;

    private readonly IDataAccessRepo dataAccessRepo;

    private const string userDataCollectionName = "Users";

    public UsersModel OfflineUser { get; set; }
    public UsersModel OnlineUser { get; set; }

    public UserRepository(IDataAccessRepo dataAccess)
    {
        dataAccessRepo = dataAccess;   

    }
    void OpenDB()
    {
        db = dataAccessRepo.GetDb();
        AllUsers = db.GetCollection<UsersModel>(userDataCollectionName);       
    }

    public async Task<UsersModel> GetUserAsync()
    {
        try
        {
            OpenDB();
            var User = await AllUsers.Query().FirstOrDefaultAsync();
            db.Dispose();

            return User;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);

            return null;
        }
       
    }
    public async Task<bool> CheckIfAnyUserExists()
    {
        OpenDB();
        int numberofUsers = await AllUsers.Query().CountAsync();
        db.Dispose();
        if (numberofUsers < 1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public async Task<UsersModel> GetUserAsync(string userEmail, string userPassword)
    {
        OpenDB();
        OfflineUser = await AllUsers.FindOneAsync(x => x.Email == userEmail && x.Password == userPassword);
        db.Dispose();
        if (OfflineUser is null)
        {
            return null;
        }
        else
        {
            return OfflineUser;
        }
    } 
    public async Task<UsersModel> GetUserAsync(string UserID)
    {
        OpenDB();
        OfflineUser = await AllUsers.FindOneAsync(x => x.Id == UserID);
        db.Dispose();
        if (OfflineUser is null)
        {
            return null;
        }
        else
        {
            return OfflineUser;
        }
    }

    /*--------- SECTION FOR OFFLINE CRUD OPERATIONS----------*/
    public async Task<bool> AddUserAsync(UsersModel user)
    {
        if (await GetUserAsync(user.Email, user.Password) is null)
        {
            OpenDB();
            if (await AllUsers.InsertAsync(user) is not null)
            {
                await AllUsers.EnsureIndexAsync(x => x.Id);
                db.Dispose();
                OfflineUser = user;
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to add User");

                db.Dispose();
                throw new Exception("Failed to add User");
            }
        }
        else
        {
            return false; //user already exists
        }
        
    }
    
    public async Task<bool> UpdateUserAsync(UsersModel user)
    {
        try
        {
            OpenDB();
            if (await AllUsers.UpdateAsync(user))
            {   
                db?.Dispose();
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to Update User");

                db.Dispose();
                return false;
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Update user Exception Message: {ex.Message}");
            return true;
        }
        
    }

    public async Task<bool> DeleteUserAsync(UsersModel user)
    {
        try 
        { 
            OpenDB();
            if(await AllUsers.DeleteAsync(user.Id))
            {
                db.Dispose();
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to delete User");
                db.Dispose();
                throw new Exception("Failed to Delete User");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update user Exception Message: {ex.Message}");
            return true;
        }
    }


    /*--------- SECTION FOR ONLINE CRUD OPERATIONS----------*/

    public async Task DropCollection()
    {
        OpenDB();

        await db.DropCollectionAsync(userDataCollectionName);
        db.Dispose();
    }
}
