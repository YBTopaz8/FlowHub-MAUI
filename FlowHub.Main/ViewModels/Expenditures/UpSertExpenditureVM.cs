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

    [ObservableProperty]
    public ExpendituresModel secondExp = new();
    [ObservableProperty]
    public ExpendituresModel thirdExp = new();
    
    [RelayCommand]
    public void PageLoaded()
    {
        _initialUserPocketMoney = ActiveUser.PocketMoney;
        _initialExpenditureAmount = SingleExpenditureDetails.AmountSpent;
        SecondExp = new();
        ThirdExp = new();
    }

    [RelayCommand]
    public async void UpSertExpenditure()
    {
        bool response = (bool)(await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You Want To Save?")))!;
        if (response)
        {
            SingleExpenditureDetails.DateSpent = SingleExpenditureDetails.DateSpent.AddHours(1);
            SecondExp.DateSpent = DateTime.UtcNow.AddHours(1);
            ThirdExp.DateSpent = DateTime.UtcNow.AddHours(1);

            CancellationTokenSource cancellationTokenSource = new();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (SingleExpenditureDetails.Id is not null)
            {
                UpdateExpFxnAsync(ToastDuration.Short, 14, cancellationTokenSource);
            }
            else
            {
                if (await AddExpFxnAsync(SingleExpenditureDetails) && SecondExp.AmountSpent != 0)
                {
                    if (await AddExpFxnAsync(SecondExp) && ThirdExp.AmountSpent != 0)
                    {
                        await AddExpFxnAsync(ThirdExp);
                        
                    }
                }
                string ToastNotifMessage = "Flow Out Added";
                IToast toast = Toast.Make(ToastNotifMessage, ToastDuration.Short, 14);
                await toast.Show(cancellationTokenSource.Token);

                NavFunctions.ReturnOnce();
            }
        }
        else
        {
            Debug.WriteLine("Action cancelled by user");
        }        
    }

    private async void UpdateExpFxnAsync(ToastDuration duration , double fontsize, CancellationTokenSource tokenSource)
    {
        SingleExpenditureDetails.UpdatedDateTime = DateTime.UtcNow;

        SingleExpenditureDetails.UpdateOnSync = true;
        await _expenditureService.UpdateExpenditureAsync(SingleExpenditureDetails);
        _expenditureService.OfflineExpendituresList.Contains(SingleExpenditureDetails);
        double difference = SingleExpenditureDetails.AmountSpent - _initialExpenditureAmount;
        var finalPocketMoney = _initialUserPocketMoney - difference;
        if (finalPocketMoney < 0)
        {
            await Shell.Current.DisplayAlert("Failed Operation", "Flow out amount is greater than your current balance", "Okay");
            Debug.WriteLine("Failed Operation", "Okay");
        }
        else
        {
            ActiveUser.PocketMoney = finalPocketMoney;
            ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
            await userService.UpdateUserAsync(ActiveUser);
            const string toastNotifMessage = "Expenditure Updated";
            var toast = Toast.Make(toastNotifMessage, ToastDuration.Short, fontsize);
            await toast.Show(tokenSource.Token);
            
            
            NavFunctions.ReturnOnce();
        }
    }

    private async Task<bool> AddExpFxnAsync(ExpendituresModel expenditure)
    {        

        expenditure.Currency = ActiveUser.UserCurrency;
        if (expenditure.AmountSpent > _initialUserPocketMoney)
        {
            await Shell.Current.DisplayAlert("Failed Operation", $"Your balance is not enough to add Flow Out {expenditure.Reason}", "Okay");
            return false;
        }
        else
        {
            _finalPocketMoney = _initialUserPocketMoney - expenditure.AmountSpent;

            if (_finalPocketMoney < 0) return false; //end the operation


            expenditure.Id = Guid.NewGuid().ToString();
            expenditure.AddedDateTime = DateTime.UtcNow;
            
            expenditure.UserId = userService.OfflineUser.UserIDOnline;

            await _expenditureService.AddExpenditureAsync(expenditure);

            _expenditureService.OfflineExpendituresList.Add(expenditure);

            ActiveUser.PocketMoney = _finalPocketMoney;
            ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
            await userService.UpdateUserAsync(ActiveUser);
            _initialUserPocketMoney = _finalPocketMoney;

            Debug.WriteLine($"Added Exp: {expenditure.Reason} and Successfully, date {expenditure.DateSpent}");
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
