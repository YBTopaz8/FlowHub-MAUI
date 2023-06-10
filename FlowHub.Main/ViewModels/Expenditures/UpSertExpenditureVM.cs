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
    public double totalss;

    [ObservableProperty]
    public double resultingBalance;

    private int expCounter;
    private double _initialUserPocketMoney = 0;
    private double _initialExpenditureAmount= 0;
    private double _finalPocketMoney = 0;
    private double _initialTotalExpAmount = 0;

    [RelayCommand]
    public void PageLoaded()
    {
        _initialUserPocketMoney = ActiveUser.PocketMoney;
        _initialExpenditureAmount = SingleExpenditureDetails.AmountSpent;
        _initialTotalExpAmount = ActiveUser.TotalExpendituresAmount;
        expCounter = 1;
        ResultingBalance = ActiveUser.PocketMoney;
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
        var totalExpCost = SingleExpenditureDetails.UnitPrice * SingleExpenditureDetails.Quantity;

        double difference = totalExpCost - _initialExpenditureAmount;

        var FinalTotalExp = _initialTotalExpAmount - difference;

        SingleExpenditureDetails.UpdatedDateTime = DateTime.UtcNow;

        SingleExpenditureDetails.UpdateOnSync = true;
        SingleExpenditureDetails.AmountSpent = totalExpCost;
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
        var ExpAmountSpent = SingleExpenditureDetails.UnitPrice * SingleExpenditureDetails.Quantity;
        expenditure.Currency = ActiveUser.UserCurrency;

        expenditure.AmountSpent = ExpAmountSpent;
        expenditure.Id = Guid.NewGuid().ToString();
        expenditure.AddedDateTime = DateTime.UtcNow;

        expenditure.UserId = userService.OfflineUser.UserIDOnline;

        if (!await _expenditureService.AddExpenditureAsync(expenditure))
            return false;
        await UpdateUserAsync(ExpAmountSpent);

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
        //bool response = (bool)(await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You Want To Cancel Action?")))!;
        //if (response)
        //{
        NavFunctions.ReturnOnce();
        //}
    }
    void GetTotals()
    {
        SingleExpenditureDetails.AmountSpent = SingleExpenditureDetails.UnitPrice * SingleExpenditureDetails.Quantity;
        Totalss = SingleExpenditureDetails.AmountSpent;
        Debug.WriteLine(SingleExpenditureDetails.AmountSpent);
    }

    [RelayCommand]
    public void GoToManageExpenditures()
    {
        NavFunctions.ReturnOnce();
    }
}
