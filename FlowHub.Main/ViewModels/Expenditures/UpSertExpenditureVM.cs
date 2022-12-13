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

    double InitialUserPocketMoney = 0;
    double InitialExpenditureAmount= 0;
    
    [RelayCommand]
    public void PageLoaded()
    {
        InitialUserPocketMoney = ActiveUser.PocketMoney;
        InitialExpenditureAmount = SingleExpenditureDetails.AmountSpent;
    }

    [RelayCommand]
    public async void UpSertExpenditure()
    {        
        CancellationTokenSource cancellationTokenSource = new();

        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;

        //var response= await AppShell.Current.DisplayAlert("Save Confirmation", "Do You want to save?", "Yes", "Cancel");
        bool response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You Want To Save?"));
        if (response)
        {
            if (SingleExpenditureDetails.Id is not null)
            {
                UpdateExpFxnAsync(duration, fontSize, cancellationTokenSource);
            }
            else
            {
                AddExpFxnAsync(duration, fontSize, cancellationTokenSource);
            }
        }
        else
        {
            Debug.WriteLine("Action cancelled by user");
        }        
    }

    private async void UpdateExpFxnAsync(ToastDuration duration , double fontsize, CancellationTokenSource tokenSource)
    {
        Debug.WriteLine("entered updated");
        await _expenditureService.UpdateExpenditureAsync(SingleExpenditureDetails);
        Debug.WriteLine("updated successfully");

        double difference = SingleExpenditureDetails.AmountSpent - InitialExpenditureAmount;
        var finalPocketMoney = InitialUserPocketMoney - difference;
        if (finalPocketMoney < 0)
        {
            Debug.WriteLine("updated but won't move here");
            await Shell.Current.DisplayAlert("Failed Operation", "Flow out amount is greater than your current balance", "Okay");
            Debug.WriteLine("Failed Operation", "Flow out amount is greater than your current balance", "Okay");
        }
        else
        {
            ActiveUser.PocketMoney = finalPocketMoney;
            Debug.WriteLine("before updated");
            await userService.UpdateUserAsync(ActiveUser);
            Debug.WriteLine("Exp updated");
            string ToastNotifMessage = "Expenditure Updated";
            var toast = Toast.Make(ToastNotifMessage, duration, fontsize);
            await toast.Show(tokenSource.Token);

            NavFunctions.ReturnOnce();
        }
    }

    private async void AddExpFxnAsync(ToastDuration duration, double fontsize, CancellationTokenSource tokenSource)
    {
        SingleExpenditureDetails.Currency = ActiveUser.UserCurrency;
        if (SingleExpenditureDetails.AmountSpent > InitialUserPocketMoney)
        {
            await Shell.Current.DisplayAlert("Failed Operation", "Flow out amount is greater than your current balance", "Okay");
        }
        else
        {
            
            SingleExpenditureDetails.Id = Guid.NewGuid().ToString();
            
            SingleExpenditureDetails.UserId = ActiveUser.Id;
            await _expenditureService.AddExpenditureAsync(SingleExpenditureDetails);

            double FinalPocketMoney = InitialUserPocketMoney - SingleExpenditureDetails.AmountSpent;
            ActiveUser.PocketMoney = FinalPocketMoney;
            await userService.UpdateUserAsync(ActiveUser);

            string ToastNotifMessage = "Flow Out Added";
            var toast = Toast.Make(ToastNotifMessage, duration, fontsize);
            await toast.Show(tokenSource.Token);

            NavFunctions.ReturnOnce();
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
