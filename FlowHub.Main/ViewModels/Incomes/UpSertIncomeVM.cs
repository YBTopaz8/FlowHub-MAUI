namespace FlowHub.Main.ViewModels.Incomes;

public partial class UpSertIncomeVM : ObservableObject
{
    private readonly IIncomeRepository incomeService;
    private readonly IUsersRepository userService;
    private readonly ManageIncomesNavs NavFunctions = new();

    public UpSertIncomeVM(IIncomeRepository incomeRepository, IUsersRepository usersRepository, IncomeModel singleIncomeDetails, string pageTitle, bool isAdd, UsersModel activeUser)
    {
        incomeService = incomeRepository;
        userService = usersRepository;
        SingleIncomeDetails = singleIncomeDetails;
        PageTitle = pageTitle;
        IsAdd = isAdd;
        ActiveUser = activeUser;
    }

    [ObservableProperty]
    IncomeModel singleIncomeDetails = new (){ DateReceived = DateTime.Now};

    [ObservableProperty]
    string pageTitle;

    [ObservableProperty]
    UsersModel activeUser;
    [ObservableProperty]
    double resultingBalance;
    [ObservableProperty]
    public bool closePopUp;

    public PopupResult ThisPopUpResult;

    double InitialUserPockerMoney;
    double InitialIncomeAmout;
    double _initialTotalIncAmount;

    public bool IsAdd { get; }

    [RelayCommand]
    public void PageLoaded()
    {
        InitialUserPockerMoney = ActiveUser.PocketMoney;
        InitialIncomeAmout = SingleIncomeDetails.AmountReceived;
        _initialTotalIncAmount = ActiveUser.TotalIncomeAmount;
        ResultingBalance = ActiveUser.PocketMoney;
    }
    [RelayCommand]
    public async Task UpSertIncome()
    {
        CancellationTokenSource cancellationTokenSource = new();
        ToastDuration duration = ToastDuration.Short;
        double fontsize = 14;
        if (SingleIncomeDetails.Id is null)
        {
            await AddIncomeAsync(duration, fontsize, cancellationTokenSource);
        }
        else
        {
            await UpdateIncomeAsync(duration, fontsize, cancellationTokenSource);
        }
        ThisPopUpResult = PopupResult.OK;
        ClosePopUp = true;
    }

    async Task UpdateIncomeAsync(ToastDuration duration, double fontSize, CancellationTokenSource tokenSource)
    {
        double difference = SingleIncomeDetails.AmountReceived - InitialIncomeAmout;

        double FinalTotalInc = _initialTotalIncAmount + difference;
        double FinalPocketMoney = InitialUserPockerMoney + difference;
        SingleIncomeDetails.UpdatedDateTime = DateTime.UtcNow;
        if (FinalPocketMoney < 0)
        {
            // show error that.. for some reason, you amount can't be -ve
        }
        else if (await incomeService.UpdateIncomeAsync(SingleIncomeDetails))
        {
            ActiveUser.TotalIncomeAmount += FinalTotalInc;
            ActiveUser.PocketMoney = FinalPocketMoney;
            ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
            await userService.UpdateUserAsync(ActiveUser);

            string toastNotifMessage = "Flow In Update";
            var toastObj = Toast.Make(toastNotifMessage, duration, fontSize);
            await toastObj.Show(tokenSource.Token);

            await ManageIncomesNavs.ReturnOnce();
        }
    }

    async Task AddIncomeAsync(ToastDuration duration, double fontSize, CancellationTokenSource tokenSource)
     {
        SingleIncomeDetails.Currency = ActiveUser.UserCurrency;
        if (SingleIncomeDetails.AmountReceived <= 0)
        {
            //call the error popup page here
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("Amount Receive must be greater than 0"));
            return;
        }
        else
        {
            SingleIncomeDetails.Id = Guid.NewGuid().ToString();
            SingleIncomeDetails.UserId = ActiveUser.Id;
            SingleIncomeDetails.AddedDateTime = DateTime.UtcNow;
            SingleIncomeDetails.UpdatedDateTime = DateTime.UtcNow;
            SingleIncomeDetails.UserId = ActiveUser.UserIDOnline;

            if (await incomeService.AddIncomeAsync(SingleIncomeDetails))
            {
                ActiveUser.TotalIncomeAmount += SingleIncomeDetails.AmountReceived;
                double FinalPocketMoney = InitialUserPockerMoney + SingleIncomeDetails.AmountReceived;
                ActiveUser.PocketMoney = FinalPocketMoney;
                ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;

                await userService.UpdateUserAsync(ActiveUser);

                string toastNotifMessage = "Flow In Added";
                var toast = Toast.Make(toastNotifMessage, duration, fontSize);
                await toast.Show(tokenSource.Token);

                await ManageIncomesNavs.ReturnOnce();

            }
        }
    }
    public void AmountReceivedChanged()
    {
        ResultingBalance = InitialUserPockerMoney - InitialIncomeAmout + SingleIncomeDetails.AmountReceived;
    }

    [RelayCommand]
    public void CancelBtn()
    {
        Debug.WriteLine("Action cancelled by user");
        ThisPopUpResult = PopupResult.Cancel;
        ClosePopUp = true;
    }
}
