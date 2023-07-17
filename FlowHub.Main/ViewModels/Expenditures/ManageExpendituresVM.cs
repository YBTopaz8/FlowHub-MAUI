
//This is the view model for the page that shows ALL expenditures

namespace FlowHub.Main.ViewModels.Expenditures;
public partial class ManageExpendituresVM : ObservableObject
{
    readonly IExpendituresRepository expendituresService;
    readonly IUsersRepository userService;

    public ManageExpendituresVM(IExpendituresRepository expendituresRepository, IUsersRepository usersRepository)
    {
        expendituresService = expendituresRepository;
        userService = usersRepository;
        ExpendituresCat = ExpenditureCategoryDescriptions.Descriptions;
        expendituresService.OfflineExpendituresListChanged += HandleExpendituresListUpdated;
    }

    [ObservableProperty]
    ObservableCollection<ExpendituresModel> expendituresList;

    [ObservableProperty]
    ObservableCollection<DateGroup> groupedExpenditures;

    [ObservableProperty]
    double totalAmount;

    [ObservableProperty]
    int totalExpenditures;

    [ObservableProperty]
    string userCurrency;

    [ObservableProperty]
    double userPocketMoney;

    [ObservableProperty]
    bool isBusy = true;

    [ObservableProperty]
    string expTitle;

    UsersModel ActiveUser = new();

    [ObservableProperty]
    bool activ;
    [ObservableProperty]
    bool showStatisticBtn;

    [ObservableProperty]
    bool isSyncing;

    [ObservableProperty]
    List<string> expendituresCat;

    public async Task PageloadedAsync()
    {
        UsersModel user = userService.OfflineUser;
        ActiveUser = user;

        UserPocketMoney = ActiveUser.PocketMoney;
        UserCurrency = ActiveUser.UserCurrency;
        await expendituresService.GetAllExpendituresAsync();
      //  filterOption = "Filter_Curr_Month";

        GetAllExp();
    }

    bool IsLoaded;
    [RelayCommand]
    //Function to show very single expenditure from DB
    public void GetAllExp()
    {
        try
        {
            if (!IsLoaded)
            {
                ExpTitle = "All Flow Outs";

                ApplyChanges();

                IsBusy = false;

                IsLoaded = true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception when loading all exp MESSAGE: {ex.Message}");
        }
    }

    private void HandleExpendituresListUpdated()
    {
        ApplyChanges();
    }

    private void ApplyChanges()
    {
        // Update expList
        var expList = expendituresService.OfflineExpendituresList
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.DateSpent).ToList();

        // Update groupedData
        var groupedData = expList.GroupBy(e => e.DateSpent.Date)
            .Select(g => new DateGroup(g.Key, g.ToList()))
            .ToList();

        // Update GroupedExpenditures
        GroupedExpenditures = new ObservableCollection<DateGroup>(groupedData);
        OnPropertyChanged(nameof(GroupedExpenditures));

#if WINDOWS
        // Update ExpendituresList
        ExpendituresList = new ObservableCollection<ExpendituresModel>(expList);
#endif

        // Update TotalAmount
        TotalAmount = expList.AsParallel().Sum(x => x.AmountSpent);

        // Update TotalExpenditures
        TotalExpenditures = expList.Count;

        // Update ShowStatisticBtn
        ShowStatisticBtn = expList.Count >= 3;
    }

    [RelayCommand]
    public async Task ShowAddExpenditurePopUp()
    {
        if (ActiveUser is null)
        {
           Debug.WriteLine("Can't Open Add Exp PopUp user is null");
           await Shell.Current.DisplayAlert("Wait", "Cannot go", "Ok");
        }
        else
        {
            var newExpenditure = new ExpendituresModel() { DateSpent = DateTime.Now };
            const string pageTitle = "Add New Flow Out";
            const bool isAdd = true;

            await AddEditExpediture(newExpenditure, pageTitle, isAdd);
        }
    }

    [RelayCommand]
    public async Task ShowEditExpenditurePopUp(ExpendituresModel expenditure)
    {
        await AddEditExpediture(expenditure, "Edit Flow Out", false);
    }
    private async Task AddEditExpediture(ExpendituresModel expenditure, string pageTitle, bool isAdd)
    {
        var NewUpSertVM = new UpSertExpenditureVM(expendituresService, userService, expenditure, pageTitle, isAdd, ActiveUser);
        await Shell.Current.ShowPopupAsync(new UpSertExpendituresPopUp(NewUpSertVM));
    }

    [RelayCommand]
    public async Task GoToSpecificStatsPage()
    {
        if (GroupedExpenditures is null)
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("No Data to visualize"));
            return;
        }

        var navParam = new Dictionary<string, object>
        {
            { "GroupedExpList", GroupedExpenditures }
        };

        await ManageExpendituresNavs.FromManageExpToSingleMonthStats(navParam);
    }

    [RelayCommand]
    public async Task DeleteExpenditureBtn(ExpendituresModel expenditure)
    {
        CancellationTokenSource cancellationTokenSource = new();
        const ToastDuration duration = ToastDuration.Short;
        const double fontSize = 14;
        string text;
        bool response = (bool)(await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You want to Delete?")))!;
        if (response)
        {
            IsBusy = true;
            expenditure.UpdatedDateTime = DateTime.UtcNow;
            expenditure.PlatformModel = DeviceInfo.Current.Model;
            var deleteResponse = await expendituresService.DeleteExpenditureAsync(expenditure); //delete the expenditure from db

            if (deleteResponse)
            {
                text = "Flow Out Deleted";

                ActiveUser.TotalExpendituresAmount -= expenditure.AmountSpent;
                ActiveUser.PocketMoney += expenditure.AmountSpent;
                UserPocketMoney += expenditure.AmountSpent;
                await userService.UpdateUserAsync(ActiveUser);
            }
            else
            {
                 text = "Flow Out Not Deleted";
            }
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token); //toast a notification about exp deletion
          //  Sorting(GlobalSortNamePosition);
            IsBusy = false;
        }
    }

    public async Task PrintExpendituresBtn()
    {
        Activ = true;
#if ANDROID
        ExpendituresList = GroupedExpenditures.SelectMany(x => x).ToObservableCollection();
#endif

        if (ExpendituresList?.Count < 1 || ExpendituresList is null)
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Cannot save an Empty list to PDF"));
            return;
        }
        string dialogueResponse = (string)await Shell.Current.ShowPopupAsync(new InputCurrencyForPrintPopUpPage("Please Select Currency", UserCurrency));
        if (dialogueResponse is "Cancel")
        {
            return;
        }

        if (dialogueResponse != UserCurrency && !Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("No Internet !\nPlease Connect to the Internet in order to save in other currencies"));
            return;
        }
        await PrintExpenditures.SaveExpenditureToPDF(ExpendituresList, ActiveUser.UserCurrency, dialogueResponse, ActiveUser.Username);
    }

    [RelayCommand]
    public static async Task CopyToClipboard(ExpendituresModel singlExp)
    {
        await Clipboard.SetTextAsync($"{singlExp.Reason} : {singlExp.AmountSpent}");
        CancellationTokenSource cancellationTokenSource = new();
        const ToastDuration duration = ToastDuration.Short;
        const double fontSize = 14;
        const string text = "Flow Out Details Copied to Clipboard";
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token); //toast a notification about exp being copied to clipboard
    }

    [RelayCommand]
    public async Task DropCollection()
    {
        await expendituresService.DropExpendituresCollection();
    }
}

public class DateGroup : List<ExpendituresModel>
{
    public DateTime Date { get; set; }
    public double TotalAmount { get; set; }
    public int TotalCount { get; set; }
    public string Currency { get; }
    public DateGroup(DateTime date, List<ExpendituresModel> expenditures) : base(expenditures)
    {
       Date = date;
       TotalAmount = expenditures.Sum(x => x.AmountSpent);
       TotalCount = expenditures.Count;
       Currency = expenditures[0].Currency;
    }
}