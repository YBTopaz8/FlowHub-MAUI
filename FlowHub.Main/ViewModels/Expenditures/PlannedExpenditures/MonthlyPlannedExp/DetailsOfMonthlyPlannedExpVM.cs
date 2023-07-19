namespace FlowHub.Main.ViewModels.Expenditures.PlannedExpenditures.MonthlyPlannedExp;

[QueryProperty(nameof(SingleMonthlyPlannedDetails), "SingleMonthlyPlanDetails")]
[QueryProperty(nameof(PageTitle), nameof(PageTitle))]
[QueryProperty(nameof(ActiveUser), "ActiveUser")]
public partial class DetailsOfMonthlyPlannedExpVM : ObservableObject
{
    private IPlannedExpendituresRepository monthlyPlannedExpService;
    private IUsersRepository userService;

    PrintDetailsMonthlyExpenditure PrintFunction;
    readonly MonthlyPlannedExpNavs NavFunctions = new();

    public DetailsOfMonthlyPlannedExpVM(IPlannedExpendituresRepository monthlyPlannedExpRepo, IUsersRepository userRepo)
    {
        monthlyPlannedExpService = monthlyPlannedExpRepo;
        userService = userRepo;
    }

    [ObservableProperty]
    PlannedExpendituresModel singleMonthlyPlannedDetails = new();

    [ObservableProperty]
    private double totalAmount;

    [ObservableProperty]
    private int totalExpenditures;

    [ObservableProperty]
    public string pageTitle;

    [ObservableProperty]
    string userCurrency;

    [ObservableProperty]
    int numberOfExpInReport;

    [ObservableProperty]
    private UsersModel activeUser;
    public ObservableCollection<ExpendituresModel> ExpendituresList { get; set; } = new();

    [ObservableProperty]
    List<ExpendituresModel> tempList = new();

    public void PageLoaded()
    {
        // UsersModel user = ActiveUser;
        UserCurrency = ActiveUser.UserCurrency;
        GetAllPlannedExpForMonth();
    }

    void GetAllPlannedExpForMonth()
    {
        try
        {
            TempList = SingleMonthlyPlannedDetails.Expenditures;

            GetTotals();
            //tempList = ExpList;
            //double tot = 0;

            //if (ExpList?.Count > 0)
            //{
            //  //  ExpendituresList.Clear();
            //    foreach (ExpendituresModel exp in ExpList)
            //    {
            //        ExpendituresList.Add(exp);

            //        tot += exp.AmountSpent;
            //    }
            //    TotalExpenditures = ExpendituresList.Count;
            //    TotalAmount = tot;
            //}
            //else
            //{
            //    //ExpendituresList.Clear();
            //}

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    async Task AddNewPlannedExpToMonthlyP()
    {
        Dictionary<string, object> navParam = new()
        {
            { "SingleMonthlyPlanned", SingleMonthlyPlannedDetails },
            { "SingleExpenditureDetails", new ExpendituresModel() },
            { "PageTitle", new string ("Add Flow Out") },
            { "CreateNewMonthlyPlanned", false },
            { "Mode", new string("AddNewExp") },
            { "IsAdd", true},
            { "ActiveUser" , ActiveUser }
        };

        await NavFunctions.ToUpSertMonthlyPlanned(navParam);
    }
    void GetTotals()
    {
        double tot = 0;
        TotalExpenditures = TempList.Count;
        foreach (ExpendituresModel item in TempList)
        {
            tot += item.AmountSpent;
        }
        TotalAmount = tot;

        // NumberOfExpInReport = TempList.Count(x => x.IncludeInReport); this is better apparently

        //NumberOfExpInReport= TempList.Where(x => x.IncludeInReport == true).Count();
    }

    public async Task GoToEditExpInMonthP(ExpendituresModel model)
    {
        Dictionary<string, object> navParam = new()
        {
            { "SingleMonthlyPlanned", SingleMonthlyPlannedDetails },
            { "SingleExpenditureDetails", model },
            { "PageTitle", new string ("Edit Flow Out") },
            { "IsAdd", false },
            { "ActiveUser" , ActiveUser }
        };
        await NavFunctions.ToUpSertMonthlyPlanned(navParam);
    }

    public async Task DeleteExpFromMonthlyP(ExpendituresModel model)
    {
        bool dialogResult = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Delete Flow Out?"));
        if (dialogResult)
        {
            CancellationTokenSource cancellationTokenSource = new();
            const ToastDuration duration = ToastDuration.Short;
            const double fontSize = 14;
            const string text = "Flow Out Deleted";
            var toast = Toast.Make(text, duration, fontSize);

            SingleMonthlyPlannedDetails.Expenditures.Remove(model);
            SingleMonthlyPlannedDetails.NumberOfExpenditures--;
            SingleMonthlyPlannedDetails.TotalAmount -= model.AmountSpent;

            SingleMonthlyPlannedDetails.UpdatedDateTime = DateTime.UtcNow;
            SingleMonthlyPlannedDetails.UpdateOnSync = true;

            await monthlyPlannedExpService.UpdatePlannedExp(SingleMonthlyPlannedDetails);
            TempList.Remove(model);

            GetTotals();
            await toast.Show(cancellationTokenSource.Token);
        }
    }

    public async Task PrintPDFandShare()
    {
        PrintFunction = new PrintDetailsMonthlyExpenditure();
        string dialogueResponse = (string)await Shell.Current.ShowPopupAsync(new InputCurrencyForPrintPopUpPage("Share PDF File? (Requires Internet)", UserCurrency));
        if (dialogueResponse is not "Cancel")
        {
            if (Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
            {
                await PrintFunction.SaveListDetailMonthlyPlanned(TempList, UserCurrency, dialogueResponse, ActiveUser.Username, SingleMonthlyPlannedDetails.Title);
            }
            else
            {
                await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("No Internet Found ! \nPlease Connect to the Internet"));
            }
        }
    }
}
