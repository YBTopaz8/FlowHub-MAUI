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
[QueryProperty(nameof(ShowAddSecondExpCheckBox), nameof(ShowAddSecondExpCheckBox))]
[QueryProperty(nameof(ActiveUser), "ActiveUser")]
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
    bool _showAddSecondExpCheckBox;

    private double _initialUserPocketMoney = 0;
    private double _initialExpenditureAmount= 0;
    private double _finalPocketMoney = 0;
    private double _initialTotalExpAmount = 0;

    [ObservableProperty]
    public ExpendituresModel secondExp = new();
    [ObservableProperty]
    public ExpendituresModel thirdExp = new();

    [ObservableProperty]
    public double totalss;

    [RelayCommand]
    public void PageLoaded()
    {
        _initialUserPocketMoney = ActiveUser.PocketMoney;
        _initialExpenditureAmount = SingleExpenditureDetails.AmountSpent;
        _initialTotalExpAmount = ActiveUser.TotalExpendituresAmount;

        SecondExp = new();
        ThirdExp = new();
    }

    [RelayCommand]
    public async void UpSertExpenditure()
    {
        /*
        bool response = (bool)(await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You Want To Save?")))!;
        if (response)
        {
            CancellationTokenSource cancellationTokenSource = new();

            if (SingleExpenditureDetails.Id is not null)
            {
                UpdateExpFxnAsync( 14, cancellationTokenSource);
            }
            else
            {

                string ToastNotifMessage = "Nothing was added";
                if (await AddExpFxnAsync(SingleExpenditureDetails))
                {
                    ToastNotifMessage = "Flow Out Added";
                    if (SecondExp.UnitPrice != 0)
                    {
                        SecondExp.DateSpent = DateTime.Now;
                        if (await AddExpFxnAsync(SecondExp) && ThirdExp.UnitPrice != 0)
                        {
                            ToastNotifMessage = "Flow Outs Added";
                            ThirdExp.DateSpent = DateTime.Now;
                            await AddExpFxnAsync(ThirdExp);

                        }
                    }
                }
                IToast toast = Toast.Make(ToastNotifMessage, ToastDuration.Short, 14);
                await toast.Show(cancellationTokenSource.Token);

                await _expenditureService.GetAllExpendituresAsync();
                NavFunctions.ReturnOnce();
            }
        }
        else
        {
            Debug.WriteLine("Action cancelled by user");
        }
        */
    }

    [RelayCommand]
    void GetTotals()
    {
        SingleExpenditureDetails.AmountSpent = SingleExpenditureDetails.UnitPrice * SingleExpenditureDetails.Quantity;
        totalss = SingleExpenditureDetails.AmountSpent;
        Debug.WriteLine(SingleExpenditureDetails.AmountSpent);
    }

    private async void UpdateExpFxnAsync( double fontsize, CancellationTokenSource tokenSource)
    {
        var totalExpCost = SingleExpenditureDetails.UnitPrice * SingleExpenditureDetails.Quantity;

        double difference = totalExpCost - _initialExpenditureAmount;

        var FinalTotalExp = _initialTotalExpAmount - difference;
        var finalPocketMoney = _initialUserPocketMoney - difference;

        if (finalPocketMoney < 0)
        {
            await Shell.Current.DisplayAlert("Failed Operation", "Flow out amount is greater than your current balance", "Okay");
            
        }
        else
        {
            SingleExpenditureDetails.UpdatedDateTime = DateTime.UtcNow;

            SingleExpenditureDetails.UpdateOnSync = true;
            SingleExpenditureDetails.AmountSpent = totalExpCost;
            if (await _expenditureService.UpdateExpenditureAsync(SingleExpenditureDetails))
            {
                await _expenditureService.GetAllExpendituresAsync();
                ActiveUser.TotalExpendituresAmount = FinalTotalExp;
                ActiveUser.PocketMoney = finalPocketMoney;
                ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
                await userService.UpdateUserAsync(ActiveUser);
                const string toastNotifMessage = "Expenditure Updated";
                var toast = Toast.Make(toastNotifMessage, ToastDuration.Short, fontsize);
                await toast.Show(tokenSource.Token);

                NavFunctions.ReturnOnce();
            }
        }
    }

    private async Task<bool> AddExpFxnAsync(ExpendituresModel expenditure)
    {        

        expenditure.Currency = ActiveUser.UserCurrency;
        var totalExpCost = expenditure.UnitPrice * expenditure.Quantity;
        if (totalExpCost > _initialUserPocketMoney)
        {
            await Shell.Current.DisplayAlert("Failed Operation", $"Your balance is not enough to add Flow Out {expenditure.Reason}", "Okay");
            return false;
        }
        else
        {
            _finalPocketMoney = _initialUserPocketMoney - totalExpCost;

            if (_finalPocketMoney < 0) return false; //end the operation

            expenditure.AmountSpent = totalExpCost;
            expenditure.Id = Guid.NewGuid().ToString();
            expenditure.AddedDateTime = DateTime.UtcNow;
            
            expenditure.UserId = userService.OfflineUser.UserIDOnline;

            if (!await _expenditureService.AddExpenditureAsync(expenditure))
                return false;

            ActiveUser.TotalExpendituresAmount += totalExpCost;
            ActiveUser.PocketMoney = _finalPocketMoney;
            ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
            await userService.UpdateUserAsync(ActiveUser);
            _initialUserPocketMoney = _finalPocketMoney;

            return true;
        }
    }

    [RelayCommand]
    public async void CancelBtn()
    {
        bool response = (bool)(await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You Want To Cancel Action?")))!;
        if (response)
        {
            NavFunctions.ReturnOnce();
        }
    }
   
    [RelayCommand]
    public void GoToManageExpenditures()
    {        
        NavFunctions.ReturnOnce();
    }
}
