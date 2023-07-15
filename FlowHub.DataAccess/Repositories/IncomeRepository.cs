using FlowHub.DataAccess.IRepositories;
using FlowHub.Models;
using LiteDB;
using LiteDB.Async;
using MongoDB.Driver;
using System.Diagnostics;

namespace FlowHub.DataAccess.Repositories;

public class IncomeRepository : IIncomeRepository
{
    LiteDatabaseAsync db;
    IMongoDatabase DBOnline;
    public List<IncomeModel> OfflineIncomesList { get; set; }
    public List<IncomeModel> OnlineIncomesList { get; set; }

    IMongoCollection<IncomeModel> AllIncomesOnline;
    ILiteCollectionAsync<IncomeModel> AllIncomes;

    readonly IDataAccessRepo dataAccessRepo;
    readonly IUsersRepository usersRepo;
    readonly IOnlineCredentialsRepository onlineDataAccessRepo;

    private const string incomesDataCollectionName = "Incomes";

    bool IsBatchUpdate;
    public event Action OfflineIncomesListChanged;

    public IncomeRepository(IDataAccessRepo dataAccess, IOnlineCredentialsRepository onlineRepository, IUsersRepository userRepository)
    {
        dataAccessRepo= dataAccess;
        usersRepo= userRepository;
        onlineDataAccessRepo = onlineRepository;
    }

    async Task<LiteDatabaseAsync> OpenDB()
    {
        db = dataAccessRepo.GetDb();
        AllIncomes = db.GetCollection<IncomeModel>(incomesDataCollectionName);
        await AllIncomes.EnsureIndexAsync(x => x.Id);
        return db;
    }

    public async Task<List<IncomeModel>> GetAllIncomesAsync()
    {
        try
        {
            await OpenDB();
            string userId = usersRepo.OfflineUser.Id;
            string userCurrency = usersRepo.OfflineUser.UserCurrency;
            if (usersRepo.OfflineUser.UserIDOnline != string.Empty)
            {
                userId = usersRepo.OfflineUser.UserIDOnline;
            }
            OfflineIncomesList = await AllIncomes.Query()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.UpdatedDateTime)
                .ToListAsync();
            db.Dispose();
            return OfflineIncomesList;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return Enumerable.Empty<IncomeModel>().ToList();
        }
    }

    async Task LoadOnlineDB()
    {
        if(Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            return;
        }
        if(OnlineIncomesList is not null)
        {
            return;
        }
        if (DBOnline is null)
        {
            onlineDataAccessRepo.GetOnlineConnection();
            DBOnline = onlineDataAccessRepo.OnlineMongoDatabase;
        }

        if(usersRepo.OnlineUser is null)
        {
            try
            {
                var filterUserCredentials = Builders<UsersModel>.Filter.Eq("Email", usersRepo.OfflineUser.Email) &
                    Builders<UsersModel>.Filter.Eq("Password", usersRepo.OfflineUser.Password);
                usersRepo.OnlineUser = await DBOnline.GetCollection<UsersModel>("Users").Find(filterUserCredentials).FirstOrDefaultAsync();
                if (usersRepo.OnlineUser is null)
                {
                    await Shell.Current.DisplayAlert("Error", "User not found", "Ok");
                    Debug.WriteLine("User not found");
                    return;
                }
                else
                {
                    usersRepo.OfflineUser.UserIDOnline = usersRepo.OnlineUser.Id;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception Message {ex.Message}");
            }
        }

        var filtersIncome = Builders<IncomeModel>.Filter.Eq("UserId", usersRepo.OfflineUser.Id) &
            Builders<IncomeModel>.Filter.Eq("Currency", usersRepo.OfflineUser.UserCurrency);

        AllIncomesOnline ??= DBOnline?.GetCollection<IncomeModel>(incomesDataCollectionName);

        OnlineIncomesList = await DBOnline.GetCollection<IncomeModel>(incomesDataCollectionName).Find(filtersIncome).ToListAsync();
    }

    bool IsSyncing;
    public async Task SynchronizeIncomesAsync()
    {
        try
        {
            await GetAllIncomesAsync();
            await LoadOnlineDB();
            if (usersRepo.OnlineUser is null)
            {
                return;
            }
            IsSyncing = true;
            IsBatchUpdate = true;

            var OfflineDict = OfflineIncomesList.ToDictionary(x => x.Id, x => x);
            var OnlineDict = OnlineIncomesList.ToDictionary(x => x.Id, x => x);
            foreach (var itemId in OfflineDict.Keys.Intersect(OnlineDict.Keys))
            {
                var offlineItem = OfflineDict[itemId];
                var onlineItem = OnlineDict[itemId];

                if (offlineItem.UpdatedDateTime > onlineItem.UpdatedDateTime)
                {
                    await UpdateIncomeOnlineAsync(offlineItem);
                }
                else if (offlineItem.UpdatedDateTime < onlineItem.UpdatedDateTime)
                {
                    await UpdateIncomeAsync(onlineItem);
                }
            }

            foreach (var itemID in OfflineDict.Keys.Except(OnlineDict.Keys))
            {
                await AddIncomeOnlineAsync(OfflineDict[itemID]);
                OnlineIncomesList.Add(OfflineDict[itemID]);

            }

            foreach (var itemID in OnlineDict.Keys.Except(OfflineDict.Keys))
            {
                await AddIncomeAsync(OnlineDict[itemID]);
                OfflineIncomesList.Add(OnlineDict[itemID]);
            }

            await usersRepo.UpdateUserOnlineGetSetLatestValues(usersRepo.OnlineUser);
            IsSyncing = false;
            IsBatchUpdate = false;
            OfflineIncomesListChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Exception Message : " + ex.Message);
        }
    }

    public async Task<bool> AddIncomeAsync(IncomeModel newIncome)
    {
        newIncome.PlatformModel = DeviceInfo.Current.Model;

        try
        {
            using (db = await OpenDB())
            {
                if (await AllIncomes.InsertAsync(newIncome) is not null)
                {
                    OfflineIncomesList.Add(newIncome);
                    Debug.WriteLine("Income inserted Locally");
                    if (!IsBatchUpdate)
                    {
                        OfflineIncomesListChanged?.Invoke();
                    }

                    if (!IsSyncing && Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        OnlineIncomesList.Add(newIncome);
                        await AddIncomeOnlineAsync(newIncome);
                    }
                    return true;
                }
                else
                {
                    Debug.WriteLine("Error while updating Income");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to handle local income: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> UpdateIncomeAsync(IncomeModel income)
    {
        income.PlatformModel = DeviceInfo.Current.Model;

        try
        {
            using (db = await OpenDB())
            {
                if (await AllIncomes.UpdateAsync(income))
                {
                    Debug.WriteLine("Income updated Locally");

                    int index = OfflineIncomesList.FindIndex(x => x.Id == income.Id);
                    OfflineIncomesList[index] = income;
                    if (!IsBatchUpdate)
                    {
                        OfflineIncomesListChanged?.Invoke();
                    }

                    if (!IsSyncing && Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        await UpdateIncomeOnlineAsync(income);
                    }
                    return true;
                }
                else
                {
                    Debug.WriteLine("Error while updating Income");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to handle local income: " + ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteIncomeAsync(IncomeModel income)
    {
        income.IsDeleted = true;

        try
        {
            using (db = await OpenDB())
            {
                if (await AllIncomes.UpdateAsync(income))
                {
                    OfflineIncomesList.Remove(income);
                    Debug.WriteLine("Income deleted Locally");
                    if (!IsBatchUpdate)
                    {
                        OfflineIncomesListChanged?.Invoke();
                    }

                    if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                    {
                        await DeleteIncomeOnlineAsync(income);
                    }
                    return true;
                }
                else
                {
                    Debug.WriteLine("Error while deleting Income");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to handle local income: " + ex.Message);
            return false;
        }
    }
 /*--------- SECTION FOR OFFLINE CRUD OPERATIONS----------*/

    async Task AddIncomeOnlineAsync(IncomeModel income)
    {
        try
        {
            await AllIncomesOnline?.InsertOneAsync(income);
            Debug.WriteLine("Income added online");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    async Task UpdateIncomeOnlineAsync(IncomeModel inc)
    {
        await AllIncomesOnline?.ReplaceOneAsync(x => x.Id == inc.Id, inc);
        Debug.WriteLine("Income Updated online");
    }

    async Task DeleteIncomeOnlineAsync(IncomeModel inc)
    {
        await AllIncomesOnline?.ReplaceOneAsync(x => x.Id == inc.Id, inc);
    }
    //think of adding a method to undelete an income

    public Task DropIncomesCollection()
    {
        throw new NotImplementedException();
    }

}
