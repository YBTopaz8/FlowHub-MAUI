namespace FlowHub.Main.ViewModels.Expenditures;

public partial class UpSertExpenditureVM : ObservableObject
{
    readonly IExpendituresRepository expenditureRepo;
    readonly IUsersRepository userRepo;

    public UpSertExpenditureVM(IExpendituresRepository expendituresRepository, IUsersRepository usersRepository, ExpendituresModel singleExpendituresDetails,
                                string pageTitle, bool isAdd, UsersModel activeUser)
    {
        expenditureRepo = expendituresRepository;
        userRepo = usersRepository;
        SingleExpenditureDetails = singleExpendituresDetails;
        PageTitle = pageTitle;
        IsAdd = isAdd;
        ActiveUser = activeUser;
        ExpenditureCategory = Enum.GetValues(typeof(ExpenditureCategory)).Cast<ExpenditureCategory>().ToList();
        userRepo.OfflineUserDataChanged += UserRepo_OfflineUserDataChanged;
    }


    [ObservableProperty]
    ExpendituresModel singleExpenditureDetails = new() { DateSpent = DateTime.Now };

    [ObservableProperty]
    string _pageTitle;

    [ObservableProperty]
    UsersModel _activeUser;

    [ObservableProperty]
    bool isAdd;

    [ObservableProperty]
    double totalAmountSpent;

    [ObservableProperty]
    double resultingBalance;

    [ObservableProperty]
    PopupResult thisPopUpResult;

    [ObservableProperty]
    bool closePopUp;

    [ObservableProperty]
    List<ExpenditureCategory> expenditureCategory;

    double _initialUserPocketMoney;
    double _initialExpenditureAmount;
    double _initialTotalExpAmount;
    public void PageLoaded()
    {
        _initialUserPocketMoney = ActiveUser.PocketMoney;
        _initialExpenditureAmount = SingleExpenditureDetails.AmountSpent;
        _initialTotalExpAmount = ActiveUser.TotalExpendituresAmount;
        if (SingleExpenditureDetails.Taxes is not null)
        {
            IsAddTaxesChecked = true;
        }

        ResultingBalance = ActiveUser.PocketMoney;
        TotalAmountSpent = SingleExpenditureDetails.AmountSpent;
    }
    private void UserRepo_OfflineUserDataChanged()
    {
        ResultingBalance = userRepo.OfflineUser.PocketMoney;
        _initialUserPocketMoney = userRepo.OfflineUser.PocketMoney;
        _initialTotalExpAmount = userRepo.OfflineUser.TotalExpendituresAmount;
        ActiveUser = userRepo.OfflineUser;
    }

    [RelayCommand]
    public async Task UpSertExpenditure()
    {
        if (ResultingBalance < 0)
        {
            ThisPopUpResult = PopupResult.Cancel;
            ClosePopUp = true;
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Not Enough balance to save"));
            return;
        }
        CancellationTokenSource cancellationTokenSource = new();
        const ToastDuration duration = ToastDuration.Short;

        SingleExpenditureDetails.UpdatedDateTime = DateTime.UtcNow;
        SingleExpenditureDetails.PlatformModel = DeviceInfo.Current.Model;
        if (SingleExpenditureDetails.Id is not null)
        {
            await UpdateExpenditureAsync(14, cancellationTokenSource, duration);
        }
        else
        {
            await AddExpenditureAsync(14, cancellationTokenSource, duration);
        }
        ThisPopUpResult = PopupResult.OK;
        ClosePopUp = true;
    }

    private async Task UpdateExpenditureAsync(double fontsize, CancellationTokenSource tokenSource, ToastDuration toastDuration)
    {
        double difference = TotalAmountSpent - _initialExpenditureAmount;
        var FinalTotalExp = _initialTotalExpAmount - difference;
        SingleExpenditureDetails.AmountSpent = TotalAmountSpent;

        if (!await expenditureRepo.UpdateExpenditureAsync(SingleExpenditureDetails))
        {
            return;
        }

        await UpdateUserAsync(FinalTotalExp);

        const string toastNotifMessage = "Flow Out Updated";
        var toast = Toast.Make(toastNotifMessage, toastDuration, fontsize);
        await toast.Show(tokenSource.Token);

        ClosePopUp = true;
    }

    private async Task<bool> AddExpenditureAsync(double fontSize, CancellationTokenSource tokenSource, ToastDuration toastDuration)
    {
        SingleExpenditureDetails.Id = Guid.NewGuid().ToString();
        SingleExpenditureDetails.Currency = ActiveUser.UserCurrency;
        SingleExpenditureDetails.AmountSpent = TotalAmountSpent;
        SingleExpenditureDetails.AddedDateTime = DateTime.UtcNow;
        SingleExpenditureDetails.UserId = userRepo.OfflineUser.UserIDOnline;

        if (!await expenditureRepo.AddExpenditureAsync(SingleExpenditureDetails))
        {
            return false;
        }

        await UpdateUserAsync(TotalAmountSpent);

        const string toastNotifMessage = "Flow Out Added";
        var toast = Toast.Make(toastNotifMessage, toastDuration, fontSize);
        await toast.Show(tokenSource.Token);
        return true;
    }

    private async Task UpdateUserAsync(double ExpAmountSpent)
    {
        ActiveUser.TotalExpendituresAmount += ExpAmountSpent;
        ActiveUser.PocketMoney = ResultingBalance;
        ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
        await userRepo.UpdateUserAsync(ActiveUser);
    }

    [RelayCommand]
    public void CancelBtn()
    {
        Debug.WriteLine("Action cancelled by user");
        ThisPopUpResult = PopupResult.Cancel;
        ClosePopUp = true;
    }

    public void UnitPriceOrQuantityChanged()
    {
        SingleExpenditureDetails.AmountSpent = SingleExpenditureDetails.UnitPrice * SingleExpenditureDetails.Quantity;
        TotalAmountSpent = SingleExpenditureDetails.AmountSpent;
        if (IsAddTaxesChecked)
        {
            ApplyTax();
        }
        ResultingBalance = _initialUserPocketMoney - SingleExpenditureDetails.AmountSpent;
    }

    [ObservableProperty]
    bool isAddTaxesChecked;

    [RelayCommand]
    public async Task HardResetUserBalance()
    {
        PopUpCloseResult result = (PopUpCloseResult)await Shell.Current.ShowPopupAsync(new InputPopUpPage(InputType.Numeric, new List<string>() { "Amount" }, "Enter New Pocket Money"));
        if (result.Result == PopupResult.OK)
        {
            double amount = (double)result.Data;

            if (amount != 0)
            {
                ActiveUser.PocketMoney = amount;
                ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
                userRepo.OfflineUser = ActiveUser;
                await userRepo.UpdateUserAsync(ActiveUser);

                CancellationTokenSource cancellationTokenSource = new();
                const ToastDuration duration = ToastDuration.Short;
                const double fontSize = 16;
                const string text = "User Balance Updated!";
                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token); //toast a notification about exp deletion

                PageLoaded();
            }
        }
    }

    public void AddTax(TaxModel tax)
    {
        SingleExpenditureDetails.Taxes ??= new List<TaxModel>();

        tax.IsChecked = true;
        if (!SingleExpenditureDetails.Taxes.Exists(t => t.Name == tax.Name))
        {
            double taxAmount = (tax.Rate / 100) * SingleExpenditureDetails.AmountSpent;
            TotalAmountSpent += taxAmount;
            ResultingBalance -= taxAmount;
            SingleExpenditureDetails.Taxes.Add(tax);
        }
    }

    public void ApplyTax()
    {
        double totalTaxPercentage = SingleExpenditureDetails.Taxes?.Sum(tax => tax.Rate) ?? 0;
        var taxedAmount = SingleExpenditureDetails.AmountSpent * (totalTaxPercentage / 100);
        TotalAmountSpent = SingleExpenditureDetails.AmountSpent + taxedAmount;
        ResultingBalance -= taxedAmount;
    }
    public void RemoveTax(TaxModel tax)
    {
        tax.IsChecked = false;
        if (SingleExpenditureDetails.Taxes.Any(t => t.Name == tax.Name))
        {
            double taxMount = (tax.Rate / 100) * SingleExpenditureDetails.AmountSpent;
            TotalAmountSpent -= taxMount;
            ResultingBalance += taxMount;
            SingleExpenditureDetails.Taxes.RemoveAll(t => t.Name == tax.Name);
        }
    }
}
