using CommunityToolkit.Maui.Core.Extensions;

//This is the view model for the HOME PAGE
namespace FlowHub.Main.ViewModels;
public partial class HomePageVM : ObservableObject
{
    private readonly IExpendituresRepository expenditureRepo;
    private readonly ISettingsServiceRepository settingsService;
    private readonly IUsersRepository userRepo;
    private  readonly IIncomeRepository incomeRepo;
    private readonly IDebtRepository debtRepo;

    public HomePageVM(IExpendituresRepository expendituresRepository, ISettingsServiceRepository settingsServiceRepo,
                    IUsersRepository usersRepository, IIncomeRepository incomeRepository,
                    IDebtRepository debtRepository)
    {
        expenditureRepo = expendituresRepository;
        settingsService = settingsServiceRepo;
        userRepo = usersRepository;
        incomeRepo = incomeRepository;
        debtRepo = debtRepository;
        expenditureRepo.OfflineExpendituresListChanged += OnChangesDetected;
        incomeRepo.OfflineIncomesListChanged += OnChangesDetected;
        userRepo.OfflineUserDataChanged += OnUserDataChanged;
    }

    private void OnUserDataChanged()
    {
        PocketMoney = userRepo.OfflineUser.PocketMoney;
    }

    private async void OnChangesDetected()
    {
        await InitializeEverything();
    }

    [ObservableProperty]
    ObservableCollection<ExpendituresModel> latestExpenditures = new();

    [ObservableProperty]
    ObservableCollection<IncomeModel> latestIncomes = new();

    [ObservableProperty]
    public int totalExp;

    [ObservableProperty]
    public string username;
    [ObservableProperty]
    public string userCurrency;
    [ObservableProperty]
    public double pocketMoney;

    [ObservableProperty]
    private UsersModel activeUser = new();

    public async Task DisplayInfo()
    {
        await SyncAndNotifyAsync();

    }
    public void GetUserData()
    {
        if (userRepo.OfflineUser is not null)
        {
            PocketMoney = userRepo.OfflineUser.PocketMoney;

        }
    }
    private async Task InitializeEverything()
    {
        try
        {
            string userId = await settingsService.GetPreference<string>("Id", "error");
            userRepo.OfflineUser = await userRepo.GetUserAsync(userId);
            var user = userRepo.OfflineUser;
            ActiveUser = user;

            Username = ActiveUser.Username;
            PocketMoney = ActiveUser.PocketMoney;
            UserCurrency = ActiveUser.UserCurrency;
            //var ListOfExp = await expenditureRepo.GetAllExpendituresAsync();
            var ListOfExp = expenditureRepo.OfflineExpendituresList;
            LatestExpenditures = ListOfExp.Count != 0
                ? ListOfExp
                .Where(x => !x.IsDeleted)
                .OrderByDescending(s => s.DateSpent)
                .Take(5)
                .ToObservableCollection()
                : new ObservableCollection<ExpendituresModel>();

            var ListOfInc = incomeRepo.OfflineIncomesList;
            LatestIncomes = ListOfInc.Count != 0
                ? ListOfInc
                .Where(predicate: x => !x.IsDeleted)
                .OrderByDescending(s => s.DateReceived)
                .Take(5)
                .ToObservableCollection()
                : new ObservableCollection<IncomeModel>();
            //await debtRepo.GetAllDebtAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error home", ex.Message, "OK");
        }
    }

    private async Task SyncAndNotifyAsync()
    {
        try
        {

            await Task.WhenAll(expenditureRepo.SynchronizeExpendituresAsync(), debtRepo.SynchronizeDebtsAsync(), incomeRepo.SynchronizeIncomesAsync());

            CancellationTokenSource cts = new();
            const ToastDuration duration = ToastDuration.Short;
            const double fontSize = 14;
            string text = "All Synced Up !";
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cts.Token);
        }
        catch (AggregateException aEx)
        {
            foreach (var ex in aEx.InnerExceptions)
            {
                await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Error when syncing " + ex.Message));
            }
        }
    }

    [RelayCommand]
    public void GetTotal()
    {
        try
        {
            var expList = expenditureRepo.OfflineExpendituresList;
            TotalExp = expList.Count;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
    [RelayCommand]
    public async Task GoToAddExpenditurePage()
    {
        var newExpenditure = new ExpendituresModel() { DateSpent = DateTime.Now };
        const string pageTitle = "Add New Flow Out";
        const bool isAdd = true;

        var NewUpSertVM = new UpSertExpenditureVM(expenditureRepo, userRepo, newExpenditure, pageTitle, isAdd, ActiveUser);
        var newUpSertExpPopUp= new UpSertExpendituresPopUp(NewUpSertVM);
        try
        {

            if (ActiveUser is null)
            {
                Debug.WriteLine("Can't Open PopUp");
                await Shell.Current.DisplayAlert("Wait", "Cannot go", "Ok");
            }
            else
            {
#if WINDOWS
                await Shell.Current.DisplayAlert("Add New Flow Out", "Please fill in the details", "Ok"); 
#endif
                var UpSertResult = (PopUpCloseResult)await Shell.Current.ShowPopupAsync(newUpSertExpPopUp);

                if (UpSertResult.Result == PopupResult.OK)
                {
                    ExpendituresModel exp = (ExpendituresModel)UpSertResult.Data;
                    //add logic if this exp is the latest in terms of datetime

                    PocketMoney -= exp.AmountSpent;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PopUp Exception on {DeviceInfo.Platform} : {ex.Message}");
        }
    }
}
