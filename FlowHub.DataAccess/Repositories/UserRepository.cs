namespace FlowHub.DataAccess.Repositories;

public class UserRepository : IUsersRepository
{
    LiteDatabaseAsync db;
    IMongoDatabase DBOnline;

    ILiteCollectionAsync<UsersModel> OfflineUserCollection;
    IMongoCollection<UsersModel> OnlineUserCollection;

    private readonly IDataAccessRepo dataAccessRepo;
    private readonly IOnlineCredentialsRepository onlineDataAccessRepo;
    
    private const string userDataCollectionName = "Users";

    public event Action OfflineUserDataChanged;

    public UsersModel OfflineUser { get; set; }
    public UsersModel OnlineUser { get; set; }

    public UserRepository(IDataAccessRepo dataAccess, IOnlineCredentialsRepository onlineRepository) //if you inject exp, inc etc here, it will lead to circular injection and app will crash :(
    {
        dataAccessRepo = dataAccess;
        onlineDataAccessRepo = onlineRepository;
    }

    void OpenDB()
    {
        db = dataAccessRepo.GetDb();
        OfflineUserCollection = db.GetCollection<UsersModel>(userDataCollectionName);
    }

    public async Task<bool> CheckIfAnyUserExists()
    {
        OpenDB();
        int numberofUsers = await OfflineUserCollection.Query().CountAsync();
        db.Dispose();
        return numberofUsers >= 1;
    }
    public async Task<UsersModel> GetUserAsync(string userEmail, string userPassword)
    {
        OpenDB();
        OfflineUser = await OfflineUserCollection.FindOneAsync(x => x.Email == userEmail && x.Password == userPassword);
        db.Dispose();
        return OfflineUser;
    }
    public async Task<UsersModel> GetUserAsync(string UserID)
    {
        OpenDB();
        OfflineUser = await OfflineUserCollection.FindOneAsync(x => x.Id == UserID);
        db.Dispose();
        OfflineUserDataChanged?.Invoke();
        return OfflineUser;
    }

    /*--------- SECTION FOR OFFLINE CRUD OPERATIONS----------*/
    public async Task<UsersModel> GetUserOnlineAsync(UsersModel user)
    {
        FilterDefinition<UsersModel> filterUserCredentials = Builders<UsersModel>.Filter.Eq("Email", user.Email.Trim()) &
                                    Builders<UsersModel>.Filter.Eq("Password", user.Password);

        if (DBOnline is null)
        {
            onlineDataAccessRepo.GetOnlineConnection();
            DBOnline = onlineDataAccessRepo.OnlineMongoDatabase;
        }
        OnlineUser ??= await DBOnline.GetCollection<UsersModel>("Users").Find(filterUserCredentials).FirstOrDefaultAsync();
        OfflineUser = OnlineUser;
        if (OnlineUser is null)
        {
            return null;
        }

        await AddUserAsync(OnlineUser);

        OfflineUserDataChanged?.Invoke();
        return OfflineUser;
    }
    public async Task<bool> AddUserAsync(UsersModel user)
    {
        if (await GetUserAsync(user.Email, user.Password) is null)
        {
            OpenDB();
            if (await OfflineUserCollection.InsertAsync(user) is not null)
            {
                _ = await OfflineUserCollection.EnsureIndexAsync(x => x.Id);
                db.Dispose();
                OfflineUser = user;
                return true;
            }
            else
            {
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
            if (await OfflineUserCollection.UpdateAsync(user))
            {
                db?.Dispose();
                OfflineUser = user;
                OfflineUserDataChanged?.Invoke();
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    await UpdateUserOnlineAsync(user);
                }
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to Update User");

                db.Dispose();
                return false;
            }
        }
        catch (Exception ex)
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
            if (await OfflineUserCollection.DeleteAsync(user.Id))
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
    public async Task<bool> AddUserOnlineAsync(UsersModel user)
    {
        FilterDefinition<UsersModel> filterUserCredentials = Builders<UsersModel>.Filter.Eq("Email", user.Email) &
                                    Builders<UsersModel>.Filter.Eq("Password", user.Password);

        if (DBOnline is null)
        {
            onlineDataAccessRepo.GetOnlineConnection();
            DBOnline = onlineDataAccessRepo.OnlineMongoDatabase;
        }

        OnlineUserCollection = DBOnline.GetCollection<UsersModel>(userDataCollectionName);

        var OnlineUser = await OnlineUserCollection.Find(filterUserCredentials).FirstOrDefaultAsync();
        if (OnlineUser != null)
        {
            // throw new Exception("This User Already Exists Online");

            return false;
        }
        else
        {
            try
            {
                await OnlineUserCollection.InsertOneAsync(user);
                OfflineUser.UserIDOnline = user.Id;
                _ = await UpdateUserAsync(OfflineUser);
                //OnlineUser = user;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to add user online : {ex.Message}");
                return false;
            }
        }
    }

    public async Task UpdateUserOnlineAsync(UsersModel user)
    {
        EnsureOnlineConnection();
        _ = await OnlineUserCollection.ReplaceOneAsync(u => u.Id == user.UserIDOnline, user);
    }

    public async Task<bool> UpdateUserOnlineGetSetLatestValues(UsersModel user)
    {
        EnsureOnlineConnection();
        FilterDefinition<UsersModel> filterUserCredentials = Builders<UsersModel>.Filter.Eq(nameof(user.Email), user.Email) &
                                    Builders<UsersModel>.Filter.Eq(nameof(user.Password), user.Password);

        var OnlineUser = await OnlineUserCollection.Find(filterUserCredentials).FirstOrDefaultAsync();
        if (OnlineUser.DateTimeOfPocketMoneyUpdate > OfflineUser.DateTimeOfPocketMoneyUpdate)
        {
            _ = await UpdateUserAsync(OnlineUser);
            OfflineUser = OnlineUser;
            return true;
        }
        else
        {
            await UpdateUserOnlineAsync(OfflineUser);
            return true;
        }
    }

    public void EnsureOnlineConnection()
    {
        if (DBOnline is null)
        {
            onlineDataAccessRepo.GetOnlineConnection();
            DBOnline = onlineDataAccessRepo.OnlineMongoDatabase;
        }

        OnlineUserCollection ??= DBOnline.GetCollection<UsersModel>(userDataCollectionName);
    }

    public async Task DropCollection()
    {
        OpenDB();

        _ = await db.DropCollectionAsync(userDataCollectionName);
        db.Dispose();
    }

    public async Task LogOutUserAsync()
    {
        OnlineUser = null;
        OfflineUser = null;
        //OfflineUserDataChanged?.Invoke();
        await DropCollection();
    }
}
