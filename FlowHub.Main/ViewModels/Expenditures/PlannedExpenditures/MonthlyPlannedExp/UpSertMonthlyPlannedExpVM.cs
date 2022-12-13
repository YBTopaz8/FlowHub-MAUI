using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Main.Platforms.NavigationsMethods;
using FlowHub.Models;
using System.Diagnostics;

namespace FlowHub.Main.ViewModels.Expenditures.PlannedExpenditures.MonthlyPlannedExp;

[QueryProperty(nameof(PageTitle), nameof(PageTitle))]
[QueryProperty(nameof(Mode), "Mode")]
[QueryProperty(nameof(CreateNewMonthlyPlanned), "CreateNewMP")]
[QueryProperty(nameof(SingleExpenditureDetails), "SingleExpenditureDetails")]
[QueryProperty(nameof(SingleMonthlyPlanned), "SingleMonthlyPlanned")]
[QueryProperty(nameof(ActiveUser), "ActiveUser")]
public partial class UpSertMonthlyPlannedExpVM : ObservableObject
{
    private readonly IPlannedExpendituresRepository monthlyPlannedExpService;
    private IUsersRepository userService;

    readonly MonthlyPlannedExpNavs NavFunctions = new();

    public UpSertMonthlyPlannedExpVM(IPlannedExpendituresRepository monthlyPlannedExpRepo, IUsersRepository userRepo)
    {
        monthlyPlannedExpService = monthlyPlannedExpRepo;
        userService = userRepo;
    }

    [ObservableProperty]
    PlannedExpendituresModel _SingleMonthlyPlanned;

    [ObservableProperty]
    private ExpendituresModel _SingleExpenditureDetails = new();

    [ObservableProperty]
    string mode;
    
    [ObservableProperty]
    bool createNewMonthlyPlanned;

    [ObservableProperty]
    public string pageTitle;

    [ObservableProperty]
    private UsersModel activeUser;

    [ObservableProperty]
    bool hasComment = false;

    double InitialSingleMonthlyPlannedExp = 0;
    double InitialExpenditureAmount = 0;

    [RelayCommand]
    public void PageLoaded()
    {
        InitialSingleMonthlyPlannedExp = SingleMonthlyPlanned.TotalAmount;
        InitialExpenditureAmount = SingleExpenditureDetails.AmountSpent;
        if(SingleExpenditureDetails.Comment is not null)
        {
            if (SingleExpenditureDetails.Comment.Equals("None"))
            {
                HasComment = false;
            }
            else
            {
                hasComment = true;
            }
        }
    }

    [RelayCommand]
    public void UpSertMonthlyPlanned()
    {
        if (SingleExpenditureDetails.Comment is null)
        {
            SingleExpenditureDetails.Comment = "None";
        }

        CancellationTokenSource cancellationTokenSource = new();

        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        var navParam = new Dictionary<string, object>
                {
                    {"SingleMonthlyPlanDetails", SingleMonthlyPlanned },
                    {"PageTitle", new string($"Edit {SingleMonthlyPlanned.MonthYear}") },
                    {"ActiveUser", ActiveUser }
                };

        if (SingleMonthlyPlanned.Id is null)
        {
            AddMonthlyPlannedExp(duration, fontSize, cancellationTokenSource);
            NavFunctions.ReturnToDetailsMonthlyPlanned(navParam);
        }
        else
        {
            
            if (SingleExpenditureDetails.Id is null)
            {
                AddExpToExistingMonthlyPlanned(duration, fontSize, cancellationTokenSource);
                NavFunctions.ReturnOnce(navParam);
            }
            else 
            {
                EditExpInExistingMonthlyPlanned(duration, fontSize, cancellationTokenSource);
                NavFunctions.ReturnOnce(navParam);
            }
                        
        }
    }

    async void AddMonthlyPlannedExp(ToastDuration duration, double fontsize, CancellationTokenSource tokenSource)
    {
        SingleMonthlyPlanned.Currency = ActiveUser.UserCurrency;
        SingleMonthlyPlanned.Id = Guid.NewGuid().ToString();
        SingleMonthlyPlanned.UserId = ActiveUser.Id;

        SingleExpenditureDetails.Id = Guid.NewGuid().ToString();
        SingleExpenditureDetails.Currency = ActiveUser.UserCurrency;
        SingleMonthlyPlanned.Expenditures.Add(SingleExpenditureDetails);
        
        SingleMonthlyPlanned.TotalAmount += SingleExpenditureDetails.AmountSpent;
        SingleMonthlyPlanned.NumberOfExpenditures += 1;

        await monthlyPlannedExpService.AddMonthlyPlannedExp(SingleMonthlyPlanned);

        string ToastNotifMessage = "Monthly Flow Out Added";
        var toast = Toast.Make(ToastNotifMessage, duration, fontsize);
        await toast.Show(tokenSource.Token);

        var navParam = new Dictionary<string, object>
        {
            {"SingleMonthlyPlanDetails", SingleMonthlyPlanned },
            {"PageTitle", new string($"Edit {SingleMonthlyPlanned.MonthYear}") },
            {"ActiveUser", ActiveUser }
        };

        NavFunctions.ReturnToDetailsMonthlyPlanned(navParam);
    }

    async void AddExpToExistingMonthlyPlanned(ToastDuration duration, double fontsize, CancellationTokenSource tokenSource)
    {
        SingleExpenditureDetails.Id = Guid.NewGuid().ToString();
        SingleExpenditureDetails.IncludeInReport = false;
        SingleExpenditureDetails.Currency = ActiveUser.UserCurrency;
        SingleMonthlyPlanned.Expenditures.Add(SingleExpenditureDetails);

        SingleMonthlyPlanned.TotalAmount += SingleExpenditureDetails.AmountSpent;
        SingleMonthlyPlanned.NumberOfExpenditures += 1;

        await monthlyPlannedExpService.UpdateMonthlyPlannedExp(SingleMonthlyPlanned);

        string ToastNotifMessage = "Flow Out Added";
        var toast = Toast.Make(ToastNotifMessage, duration, fontsize);
        await toast.Show(tokenSource.Token);
    }

    async void EditExpInExistingMonthlyPlanned(ToastDuration duration, double fontsize, CancellationTokenSource tokenSource)
    {        
        if (SingleMonthlyPlanned.Expenditures.Contains(SingleExpenditureDetails))
        {
            var difference = InitialExpenditureAmount - SingleExpenditureDetails.AmountSpent;
            SingleMonthlyPlanned.TotalAmount = InitialSingleMonthlyPlannedExp - difference;
        }

        await monthlyPlannedExpService.UpdateMonthlyPlannedExp(SingleMonthlyPlanned);
        string ToastNotifMessage = "Flow Out Edited";
        var toast = Toast.Make(ToastNotifMessage, duration, fontsize);
        await toast.Show(tokenSource.Token);
    }
}
