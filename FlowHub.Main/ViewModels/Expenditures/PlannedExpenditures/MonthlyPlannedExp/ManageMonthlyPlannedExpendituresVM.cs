using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Main.Platforms.NavigationsMethods;
using FlowHub.Main.PopUpPages;
using FlowHub.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace FlowHub.Main.ViewModels.Expenditures.PlannedExpenditures.MonthlyPlannedExp;

public partial class ManageMonthlyMonthlyPlannedExpendituresVM : ObservableObject
{
    private IPlannedExpendituresRepository monthlyPlannedExpService;
    private IUsersRepository userService;

    readonly MonthlyPlannedExpNavs NavFunctions = new();

    public ManageMonthlyMonthlyPlannedExpendituresVM(IPlannedExpendituresRepository monthlyPlannedExpRepo, IUsersRepository userRepo)
    {
        monthlyPlannedExpService = monthlyPlannedExpRepo;
        userService = userRepo;
    }
    public ObservableCollection<PlannedExpendituresModel> MonthlyPlannedExpList { get; set; } = new();

    UsersModel ActiveUser = new();

    private List<PlannedExpendituresModel> tempList = new();


    [RelayCommand]
    public async void PageLoaded()
    {
        var user = userService.OfflineUser;
        ActiveUser = user;
        tempList = await monthlyPlannedExpService.GetAllMonthlyPlannedExp();
       
        GetAllMonthlyPlanned();

    }

    private void GetAllMonthlyPlanned()
    {
        try
        {
            List<PlannedExpendituresModel> ListMonthlyPlannedExp = tempList.FindAll(x => x.IsMonthlyPlanned == true)
                .Select(model => new { Exp = model, Sort = DateTime.ParseExact(model.MonthYear, "MMMM, yyyy", CultureInfo.InvariantCulture) })
                .OrderBy(model => model.Sort)
                .Select(model => model.Exp)
                .ToList();
            if (ListMonthlyPlannedExp?.Count > 0)
            {
                MonthlyPlannedExpList.Clear();
                foreach (PlannedExpendituresModel mPlannedExp in ListMonthlyPlannedExp)
                {
                    MonthlyPlannedExpList.Add(mPlannedExp);
                }

            }
            else
            {
                MonthlyPlannedExpList.Clear();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");

        }
    }

    [RelayCommand]
    public async void ShowInputMonthYearPopupPage()
    {
        var monthYear = (string)await Shell.Current.ShowPopupAsync(new InputMonthAndYearPopUp());
        if (monthYear.Equals("Cancel"))
        {
            Debug.WriteLine("Cancelled");
        }
        else
        {
            Debug.WriteLine($"{monthYear}");
            if (ActiveUser is null)
            {
                Debug.WriteLine("Can't go");
                await Shell.Current.DisplayAlert("Wait", "Please wait", "Ok");
            }
            else
            {
                var navParam = new Dictionary<string, object>
                {
                    {"SingleMonthlyPlanned", new PlannedExpendituresModel {MonthYear = monthYear, IsMonthlyPlanned=true, Expenditures = new List<ExpendituresModel>()} },
                    {"SingleExpenditureDetails", new ExpendituresModel () },
                    {"PageTitle", new string ($"Planned Flow Out: {monthYear}") },
                    
                    {"ActiveUser" , ActiveUser }
                };

                NavFunctions.ToUpSertMonthlyPlanned(navParam);
            }
        }
    }

    [RelayCommand]
    public void GoToViewMonthlyPlannedExp(PlannedExpendituresModel monthlyPlannedExp)
    {
        var navParam = new Dictionary<string, object>
        {
            {"SingleMonthlyPlanDetails", monthlyPlannedExp },
            {"PageTitle", new string($"View {monthlyPlannedExp.MonthYear}") },
            { "ActiveUser", ActiveUser }
        };
        NavFunctions.ToDetailsMonthlyPlanned(navParam);
    }

    [RelayCommand]
    public async void DeleteMonthlyPlannedExp(PlannedExpendituresModel monthlyPlannedExp)
    {
        CancellationTokenSource cancellationTokenSource = new();
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        string text = $"Monthly Planned For {monthlyPlannedExp.MonthYear} Deleted";
        var toast = Toast.Make(text, duration, fontSize);

        var deleteRespone = await monthlyPlannedExpService.DeleteMonthlyPlannedExp(monthlyPlannedExp);
        MonthlyPlannedExpList.Remove(monthlyPlannedExp);
        await toast.Show(cancellationTokenSource.Token);
        //GetAllMonthlyPlanned();
    }
}
