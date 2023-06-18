using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Models;
using System.Diagnostics;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using FlowHub.Main.Platforms.NavigationMethods;
using CommunityToolkit.Maui.Views;
using FlowHub.Main.PopUpPages;

namespace FlowHub.Main.ViewModels.Expenditures;

[QueryProperty(nameof(SingleExpenditureDetails), "SingleExpenditureDetails")]
[QueryProperty(nameof(PageTitle), nameof(PageTitle))]
[QueryProperty(nameof(ActiveUser), "ActiveUser")]
[QueryProperty(nameof(IsAdd), "IsAdd")]
public partial class UpSertExpenditureVM : ObservableObject
{
    private readonly IExpendituresRepository _expenditureService;
    private readonly IUsersRepository userService;
    private readonly ManageExpendituresNavs NavFunctions = new();
    public UpSertExpenditureVM(IExpendituresRepository expendituresRepository, IUsersRepository usersRepository)
    {
        _expenditureService = expendituresRepository;
        userService = usersRepository;
    }

    [ObservableProperty]
    private ExpendituresModel _SingleExpenditureDetails = new(){ DateSpent = DateTime.Now };

    [ObservableProperty]
    private string _pageTitle;

    [ObservableProperty]
    private UsersModel _activeUser;

    [ObservableProperty]
    private bool isAdd;

    [ObservableProperty]
    private bool addAnotherExp;

    [ObservableProperty]
    public double totalAmountSpent;

    [ObservableProperty]
    public double resultingBalance;

    private int expCounter;
    private double _initialUserPocketMoney;
    private double _initialExpenditureAmount;

    private double _initialTotalExpAmount;

    [RelayCommand]
    public void PageLoaded()
    {
        _initialUserPocketMoney = ActiveUser.PocketMoney;
        _initialExpenditureAmount = SingleExpenditureDetails.AmountSpent;
        _initialTotalExpAmount = ActiveUser.TotalExpendituresAmount;
        if (SingleExpenditureDetails.Taxes is not null)
        {
            IsAddTaxesChecked = true;
        }
        expCounter = 1;
        ResultingBalance = ActiveUser.PocketMoney;
        TotalAmountSpent = SingleExpenditureDetails.AmountSpent;
    }

    [RelayCommand]
    public async void UpSertExpenditure()
    {
        if (ResultingBalance < 0)
        {
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("Not Enough balance to save"));
            return;
        }

        bool response = (bool)(await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You Want To Save?")))!;
        if (!response)
        {
            Debug.WriteLine("Action cancelled by user");
            return;
        }

        CancellationTokenSource cancellationTokenSource = new();

        if (SingleExpenditureDetails.Id is not null)
        {
            UpdateExpenditureAsync(14, cancellationTokenSource);
            return;
        }

        string ToastNotifMessage = (await AddExpenditureAsync(SingleExpenditureDetails)) ? "Flow Out Added" : "Nothing was added";

        IToast toast = Toast.Make(ToastNotifMessage, ToastDuration.Short, 14);
        await toast.Show(cancellationTokenSource.Token);

        if (!AddAnotherExp)
        {
            NavFunctions.ReturnOnce();
            return;
        }

        expCounter++;
        PageTitle = $"Add Flow Out N° {expCounter}";
        SingleExpenditureDetails = new() { DateSpent = DateTime.Now };
        AddAnotherExp = false;
    }
    private async void UpdateExpenditureAsync( double fontsize, CancellationTokenSource tokenSource)
    {
        double difference = TotalAmountSpent - _initialExpenditureAmount;

        var FinalTotalExp = _initialTotalExpAmount - difference;

        SingleExpenditureDetails.UpdatedDateTime = DateTime.UtcNow;

        SingleExpenditureDetails.UpdateOnSync = true;
        SingleExpenditureDetails.AmountSpent = TotalAmountSpent;
        if (await _expenditureService.UpdateExpenditureAsync(SingleExpenditureDetails))
        {
            await _expenditureService.GetAllExpendituresAsync();

            await UpdateUserAsync(FinalTotalExp);

            const string toastNotifMessage = "Expenditure Updated";
            var toast = Toast.Make(toastNotifMessage, ToastDuration.Short, fontsize);
            await toast.Show(tokenSource.Token);

            NavFunctions.ReturnOnce();
        }
    }

    private async Task<bool> AddExpenditureAsync(ExpendituresModel expenditure)
    {
        expenditure.Currency = ActiveUser.UserCurrency;

        expenditure.AmountSpent = TotalAmountSpent;
        expenditure.Id = Guid.NewGuid().ToString();
        expenditure.AddedDateTime = DateTime.UtcNow;

        expenditure.UserId = userService.OfflineUser.UserIDOnline;

        if (!await _expenditureService.AddExpenditureAsync(expenditure))
            return false;
        await UpdateUserAsync(TotalAmountSpent);

        return true;
    }

    private async Task UpdateUserAsync(double ExpAmountSpent)
    {
        ActiveUser.TotalExpendituresAmount += ExpAmountSpent;
        ActiveUser.PocketMoney = ResultingBalance;
        ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
        await userService.UpdateUserAsync(ActiveUser);
    }

    [RelayCommand]
    public void CancelBtn()
    {
        NavFunctions.ReturnOnce();
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
            SingleExpenditureDetails.Taxes.RemoveAll(t=>t.Name == tax.Name);
        }
    }

    [RelayCommand]
    public async void HardResetUserBalance()
    {
        PopUpCloseResult result = (PopUpCloseResult)await Shell.Current.ShowPopupAsync(new InputPopUpPage(InputType.Numeric, new List<string>() { "Amount" }, "Enter New Pocket Money"));
        if (result.Result == PopupResult.OK)
        {
            double amount = (double)result.Data;

            if (amount != 0)
            {
                ActiveUser.PocketMoney = amount;
                ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
                userService.OfflineUser = ActiveUser;
                await userService.UpdateUserAsync(ActiveUser);

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

    [RelayCommand]
    public void GoToManageExpenditures()
    {
        NavFunctions.ReturnOnce();
    }
}
