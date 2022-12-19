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
using Microsoft.Maui;

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
    public string pageTitle;

    [ObservableProperty]
    private UsersModel activeUser;

    [ObservableProperty]
    bool showAddSecondExpCheckBox;

    double InitialUserPocketMoney = 0;
    double InitialExpenditureAmount= 0;
    double FinalPocketMoney = 0;

    [ObservableProperty]
    public ExpendituresModel secondExp = new();
    [ObservableProperty]
    public ExpendituresModel thirdExp = new();
    
    [RelayCommand]
    public void PageLoaded()
    {
        InitialUserPocketMoney = ActiveUser.PocketMoney;
        InitialExpenditureAmount = SingleExpenditureDetails.AmountSpent;
        SecondExp = new();
        ThirdExp = new();
    }

    [RelayCommand]
    public async void UpSertExpenditure()
    {
        bool response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You Want To Save?"));
        if (response)
        {
            SingleExpenditureDetails.DateSpent = SingleExpenditureDetails.DateSpent.AddHours(1);
            SecondExp.DateSpent = DateTime.UtcNow.AddHours(1);
            ThirdExp.DateSpent = DateTime.UtcNow.AddHours(1);

            CancellationTokenSource cancellationTokenSource = new();

            ToastDuration duration = ToastDuration.Short;
            double fontSize = 14;
            if (SingleExpenditureDetails.Id is not null)
            {
                UpdateExpFxnAsync(duration, fontSize, cancellationTokenSource);
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
                IToast toast = Toast.Make(ToastNotifMessage, duration, fontSize);
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
        await _expenditureService.UpdateExpenditureAsync(SingleExpenditureDetails);

        double difference = SingleExpenditureDetails.AmountSpent - InitialExpenditureAmount;
        var finalPocketMoney = InitialUserPocketMoney - difference;
        if (finalPocketMoney < 0)
        {
            await Shell.Current.DisplayAlert("Failed Operation", "Flow out amount is greater than your current balance", "Okay");
            Debug.WriteLine("Failed Operation", "Flow out amount is greater than your current balance", "Okay");
        }
        else
        {
            ActiveUser.PocketMoney = finalPocketMoney;
            await userService.UpdateUserAsync(ActiveUser);
            string ToastNotifMessage = "Expenditure Updated";
            var toast = Toast.Make(ToastNotifMessage, duration, fontsize);
            await toast.Show(tokenSource.Token);

            NavFunctions.ReturnOnce();
        }
    }

    private async Task<bool> AddExpFxnAsync(ExpendituresModel Expenditure)
    {        
        Expenditure.Currency = ActiveUser.UserCurrency;
        if (Expenditure.AmountSpent > InitialUserPocketMoney)
        {
            await Shell.Current.DisplayAlert("Failed Operation", $"Your balance is not enough to add Flow Out {Expenditure.Reason}", "Okay");
            return false;
        }
        else
        {
            FinalPocketMoney = InitialUserPocketMoney - Expenditure.AmountSpent;
            
            if (FinalPocketMoney > 0)
            {
                Expenditure.Id = Guid.NewGuid().ToString();
            
                Expenditure.UserId = ActiveUser.Id;
                await _expenditureService.AddExpenditureAsync(Expenditure);

                ActiveUser.PocketMoney = FinalPocketMoney;
                await userService.UpdateUserAsync(ActiveUser);
                InitialUserPocketMoney = FinalPocketMoney;
                Debug.WriteLine($"Added Exp: {Expenditure.Reason} and Successfully, date {Expenditure.DateSpent}");
                return true;
            }
            return false;   
        }
    }

    [RelayCommand]
    public async void CancelBtn()
    {
        bool response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You Want To Cancel Action?"));
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
