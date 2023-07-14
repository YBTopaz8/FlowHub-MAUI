using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Main.Platforms.NavigationMethods;
using FlowHub.Main.PopUpPages;
using FlowHub.Models;
using System.Diagnostics;

namespace FlowHub.Main.ViewModels.Incomes;

[QueryProperty(nameof(PageTitle), nameof(PageTitle))]
[QueryProperty(nameof(SingleIncomeDetails), "SingleIncomeDetails")]
[QueryProperty(nameof(ActiveUser), "ActiveUser")]
public partial class UpSertIncomeVM : ObservableObject
{
    private readonly IIncomeRepository incomeService;
    private readonly IUsersRepository userService;
    private readonly ManageIncomesNavs NavFunctions = new();

    public UpSertIncomeVM(IIncomeRepository incomeRepository, IUsersRepository usersRepository)
    {
        incomeService = incomeRepository;
        userService = usersRepository;
    }

    [ObservableProperty]
    private IncomeModel singleIncomeDetails;

    [ObservableProperty]
    private string pageTitle;

    [ObservableProperty]
    private UsersModel activeUser;

    double InitialUserPockerMoney;
    double InitialIncomeAmout;
    double _initialTotalIncAmount;

    [RelayCommand]
    public void PageLoaded()
    {
        InitialUserPockerMoney = ActiveUser.PocketMoney;
        InitialIncomeAmout = SingleIncomeDetails.AmountReceived;
        _initialTotalIncAmount = ActiveUser.TotalIncomeAmount;
    }
    [RelayCommand]
    public async Task UpSertIncome()
    {
        CancellationTokenSource cancellationTokenSource = new();
        ToastDuration duration = ToastDuration.Short;
        double fontsize = 14;
        bool response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You Want To Save?"));
        if (response)
        {
            if (SingleIncomeDetails.Id is null)
            {
                await AddIncFxnAsync(duration, fontsize, cancellationTokenSource);
            }
            else
            {
                await UpdateIncFxnAsync(duration, fontsize, cancellationTokenSource);
            }
        }
        else
        {
            Debug.WriteLine("Action cancelled by user");
        }
    }

    async Task UpdateIncFxnAsync(ToastDuration duration, double fontSize, CancellationTokenSource tokenSource)
    {
        double difference = SingleIncomeDetails.AmountReceived - InitialIncomeAmout;

        double FinalTotalInc = _initialTotalIncAmount + difference;
        double FinalPocketMoney = InitialUserPockerMoney + difference;
        if (FinalPocketMoney < 0)
        {
            // show error that.. for some reason, you amount can't be -ve
        }
        else
        {
            if (await incomeService.UpdateIncomeAsync(SingleIncomeDetails))
            {
                ActiveUser.TotalIncomeAmount += FinalTotalInc;
                ActiveUser.PocketMoney = FinalPocketMoney;
                ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
                await userService.UpdateUserAsync(ActiveUser);

                string toastNotifMessage = "Flow In Update";
                var toastObj = Toast.Make(toastNotifMessage, duration, fontSize);
                await toastObj.Show(tokenSource.Token);

                await NavFunctions.ReturnOnce();
            }
        }
    }

     async Task AddIncFxnAsync(ToastDuration duration, double fontSize, CancellationTokenSource tokenSource)
     {
        SingleIncomeDetails.Currency = ActiveUser.UserCurrency;
        if (SingleIncomeDetails.AmountReceived <= 0)
        {
            //call the error popup page here
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("Error, I'll put the reason later"));
        }
        else
        {
            SingleIncomeDetails.Id = Guid.NewGuid().ToString();
            SingleIncomeDetails.UserId = ActiveUser.Id;
            SingleIncomeDetails.AddedDateTime = DateTime.UtcNow;
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

                await NavFunctions.ReturnOnce();
            }
        }
    }

    [RelayCommand]
    public async Task CancelBtn()
    {
        bool response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You Want To Cancel Action?"));
        if(response)
        {
            await NavFunctions.ReturnOnce();
        }
    }
}
