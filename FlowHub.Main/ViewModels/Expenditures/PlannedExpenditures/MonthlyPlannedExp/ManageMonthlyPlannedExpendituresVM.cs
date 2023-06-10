using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Main.PDF_Classes;
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
    PrintDetailsMonthlyExpenditure PrintFunction;

    public ManageMonthlyMonthlyPlannedExpendituresVM(IPlannedExpendituresRepository monthlyPlannedExpRepo, IUsersRepository userRepo)
    {
        monthlyPlannedExpService = monthlyPlannedExpRepo;
        userService = userRepo;
    }
    public ObservableCollection<PlannedExpendituresModel> MonthlyPlannedExpList { get; set; } = new();

    UsersModel ActiveUser = new();

    private List<PlannedExpendituresModel> tempList = new();

    [ObservableProperty]
    private bool isBusy;

    public List<List<ExpendituresModel>> ListOfExpenditures;
    public List<string> ListOfExpTitles;

    [RelayCommand]
    public async void PageLoaded()
    {
        var user = userService.OfflineUser;
        ActiveUser = user;
        _ = await monthlyPlannedExpService.GetAllPlannedExp();
        GetAllMonthlyPlanned();
    }

    private void GetAllMonthlyPlanned()
    {
        try
        {
            var ListMonthlyPlannedExp = monthlyPlannedExpService.OfflinePlannedExpendituresList
                .FindAll(x => x.IsMonthlyPlanned)
                .Select(model => new { Exp = model, Sort = DateTime.ParseExact(model.Title, "MMMM, yyyy", CultureInfo.InvariantCulture) })
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
                    {"SingleMonthlyPlanned", new PlannedExpendituresModel {Title = monthYear, IsMonthlyPlanned=true, Expenditures = new List<ExpendituresModel>()} },
                    {"SingleExpenditureDetails", new ExpendituresModel () },
                    { "IsAdd", true },
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
            {"PageTitle", new string($"View {monthlyPlannedExp.Title}") },
            { "ActiveUser", ActiveUser }
        };
        NavFunctions.ToDetailsMonthlyPlanned(navParam);
    }

    [RelayCommand]
    public async void DeleteMonthlyPlannedExp(PlannedExpendituresModel monthlyPlannedExp)
    {
        bool dialogResult = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Delete Monthly Planned Flow?"));
        if (dialogResult)
        {
            CancellationTokenSource cancellationTokenSource = new();
            ToastDuration duration = ToastDuration.Short;
            double fontSize = 14;
            string text = $"Monthly Planned For {monthlyPlannedExp.Title} Deleted";
            var toast = Toast.Make(text, duration, fontSize);

            await monthlyPlannedExpService.DeletePlannedExp(monthlyPlannedExp.Id);
            monthlyPlannedExpService.OfflinePlannedExpendituresList.Remove(monthlyPlannedExp);
            MonthlyPlannedExpList.Remove(monthlyPlannedExp);
            await toast.Show(cancellationTokenSource.Token);
        }
        //GetAllMonthlyPlanned();
    }

    [RelayCommand]
    public async void SyncPlannedExpTest()
    {
        IsBusy = true;
        Debug.WriteLine("Begin Sync");
        if (await monthlyPlannedExpService.SynchronizePlannedExpendituresAsync(ActiveUser.Email, ActiveUser.Password))
        {
            PageLoaded();

            IsBusy = false;
        }
        Debug.WriteLine("End Sync");
    }

    public async Task PrintPDFandShare(List<List<ExpendituresModel>> ListofListofExps, List<string> listofExpTitles)
    {
        Debug.WriteLine(ListofListofExps.Count);
        Debug.WriteLine(listofExpTitles.Count);
        PrintFunction = new PrintDetailsMonthlyExpenditure();
        string dialogueResponse = (string)await Shell.Current.ShowPopupAsync(new InputCurrencyForPrintPopUpPage("Share PDF File? (Requires Internet)", ActiveUser.UserCurrency));
        if (dialogueResponse is not "Cancel")
        {
            if (Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
            {
                await PrintFunction.SaveListDetailMonthlyPlanned(ListofListofExps, ActiveUser.UserCurrency, dialogueResponse, ActiveUser.Username, listofExpTitles);
            }
            else
            {
                await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("No Internet Found ! \nPlease Connect to the Internet"));
            }
        }
    }
}
