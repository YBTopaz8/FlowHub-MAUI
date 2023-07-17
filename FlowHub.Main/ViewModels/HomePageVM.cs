using CommunityToolkit.Maui.Core.Extensions;

//This is the view model for the HOME PAGE
namespace FlowHub.Main.ViewModels;

[QueryProperty(nameof(TotalExp), nameof(TotalExp))]
public partial class HomePageVM : ObservableObject
{
    public readonly IExpendituresRepository expenditureRepo;
    private readonly ISettingsServiceRepository settingsService;
    private readonly IUsersRepository userService;
    public readonly IIncomeRepository incomeRepo;
    private readonly IDebtRepository debtRepo;
    private readonly HomePageNavs NavFunction = new();

    public HomePageVM(IExpendituresRepository expendituresRepository, ISettingsServiceRepository settingsServiceRepo,
                    IUsersRepository usersRepository, IIncomeRepository incomeRepository,
                    IDebtRepository debtRepository)
    {
        expenditureRepo = expendituresRepository;
        settingsService = settingsServiceRepo;
        userService = usersRepository;
        incomeRepo = incomeRepository;
        debtRepo = debtRepository;
        expenditureRepo.OfflineExpendituresListChanged += OnChangesDetected;
        incomeRepo.OfflineIncomesListChanged += OnChangesDetected;
    }

    private async void OnChangesDetected()
    {
        await InitializeEverything();
    }

    [ObservableProperty]
    ObservableCollection<ExpendituresModel> _latestExpenditures;
    [ObservableProperty]
    ObservableCollection<IncomeModel> _latestIncomes;

    [ObservableProperty]
    public int totalExp;

    [ObservableProperty]
    public string username;
    [ObservableProperty]
    public string userCurrency;
    [ObservableProperty]
    public double pocketMoney;

    [ObservableProperty]
    private UsersModel activeUser = new ();

    public async Task DisplayInfo()
    {
        await InitializeEverything();

        await SyncAndNotifyAsync();
    }

    private async Task InitializeEverything()
    {
        string Id = await settingsService.GetPreference<string>("Id", "error");

        var user = userService.OfflineUser;
        ActiveUser = user;

        Username = ActiveUser.Username;
        PocketMoney = ActiveUser.PocketMoney;
        UserCurrency = ActiveUser.UserCurrency;
        var ListOfExp = await expenditureRepo.GetAllExpendituresAsync();

        LatestExpenditures = ListOfExp.Count != 0
            ? ListOfExp
            .Where(x => !x.IsDeleted)
            .OrderByDescending(s => s.DateSpent)
            .Take(5)
            .ToObservableCollection()
            : new ObservableCollection<ExpendituresModel>();

        var ListOfInc = await incomeRepo.GetAllIncomesAsync();
        LatestIncomes = ListOfInc.Count != 0
            ? ListOfInc
            .Where(predicate: x => !x.IsDeleted)
            .OrderByDescending(s => s.DateReceived)
            .Take(5)
            .ToObservableCollection()
            : new ObservableCollection<IncomeModel>();
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
        if (ActiveUser is null)
        {
            Debug.WriteLine("Can't Open PopUp");
            await Shell.Current.DisplayAlert("Wait", "Cannot go", "Ok");
        }
        else
        {
            var newExpenditure = new ExpendituresModel() { DateSpent = DateTime.Now };
            const string pageTitle = "Add New Flow Out";
            const bool isAdd = true;

            var NewUpSertVM = new UpSertExpenditureVM(expenditureRepo, userService, newExpenditure, pageTitle, isAdd, ActiveUser);
            var UpSertResult = (PopUpCloseResult)await Shell.Current.ShowPopupAsync(new UpSertExpendituresPopUp(NewUpSertVM));

            if (UpSertResult.Result == PopupResult.OK)
            {
                ExpendituresModel exp = (ExpendituresModel)UpSertResult.Data;
                //add logic if this exp is the latest in terms of datetime
                LatestExpenditures.Add(exp);
                LatestExpenditures = LatestExpenditures.OrderByDescending(s => s.DateSpent).Take(5).ToObservableCollection();
                PocketMoney -= exp.AmountSpent;
            }
        }
    }
}
