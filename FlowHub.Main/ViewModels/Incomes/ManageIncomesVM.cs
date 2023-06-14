using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Main.Platforms.NavigationMethods;
using FlowHub.Main.PopUpPages;
using FlowHub.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace FlowHub.Main.ViewModels.Incomes;

public partial class ManageIncomesVM : ObservableObject
{
    private readonly IIncomeRepository incomeService;
    private readonly IUsersRepository userService;

    private readonly ManageIncomesNavs NavFunctions = new();

    public ManageIncomesVM(IIncomeRepository incomeRepository, IUsersRepository usersRepository)
    {
        incomeService = incomeRepository;
        userService = usersRepository;
    }

    public ObservableCollection<IncomeModel> IncomesList { get; set; } = new();

    [ObservableProperty]
    private double totalAmount;

    [ObservableProperty]
    private int totalIncomes;

    [ObservableProperty]
    private string userCurrency;

    [ObservableProperty]
    private double userPockerMoney;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string incTitle;

    private UsersModel ActiveUser = new();

    [RelayCommand]
    public async void PageLoaded()
    {
        var user = userService.OfflineUser;
        ActiveUser = user;
        UserPockerMoney = ActiveUser.PocketMoney;
        UserCurrency = ActiveUser.UserCurrency;
        await incomeService.GetAllIncomesAsync();
        FilterGetIncOfCurrentMonth();
    }

    [RelayCommand]
    public void FilterGetIncOfCurrentMonth()
    {
        try
        {
            IsBusy = true;
            double totalAmountFromList = 0;
            var incOfCurrentMonth = incomeService.OfflineIncomesList.FindAll(x => x.DateReceived.Month == DateTime.Today.Month)
                .ToList();
            if (incOfCurrentMonth?.Count > 0)
            {
                IsBusy = false;
                IncomesList.Clear();
                foreach (IncomeModel inc in incOfCurrentMonth)
                {
                    IncomesList.Add(inc);
                    totalAmountFromList += inc.AmountReceived;
                }
                TotalAmount = totalAmountFromList;
                TotalIncomes = IncomesList.Count;
                IncTitle = $"Flow In For {DateTime.Now:MMM - yyyy}";
            }
            else
            {
                IsBusy = false;
                IncomesList.Clear();
                TotalIncomes = IncomesList.Count;
                IncTitle = $"Flow Ins For {DateTime.Now:MMM - yyyy}";
                TotalAmount = 0;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    public async void FilterGetAllIncomes()
    {
        try
        {
            IsBusy = true;
            double totalAmountFromList = 0;
            var AllIncomes = incomeService.OfflineIncomesList;
            if (AllIncomes?.Count > 0)
            {
                IsBusy = false;
                IncomesList.Clear();
                foreach (IncomeModel inc in AllIncomes)
                {
                    IncomesList.Add(inc);
                    totalAmountFromList += inc.AmountReceived;
                }
                TotalAmount = totalAmountFromList;
                TotalIncomes = IncomesList.Count;
                IncTitle = $"All Flow Ins";
                if (ActiveUser.TotalIncomeAmount == 0)
                {
                    ActiveUser.TotalIncomeAmount = totalAmountFromList;
                    await userService.UpdateUserAsync(ActiveUser);
                }
            }
            else
            {
                IsBusy = false;
                IncomesList.Clear();
                TotalIncomes = IncomesList.Count;
                IncTitle = $"All Flow Ins";
                TotalAmount = 0;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    public void FilterGetIncOfToday()
    {
        try
        {
            IsBusy = true;
            double totalAmountFromList = 0;
            var incOfToday = incomeService.OfflineIncomesList.FindAll(x => x.DateReceived.Day == DateTime.Today.Day)
                .ToList();
            if (incOfToday?.Count > 0)
            {
                IsBusy = false;
                IncomesList.Clear();
                foreach (IncomeModel inc in incOfToday)
                {
                    IncomesList.Add(inc);
                    totalAmountFromList += inc.AmountReceived;
                }
                TotalAmount = totalAmountFromList;
                TotalIncomes = IncomesList.Count;
                IncTitle = $"Today's Flow Ins";
            }
            else
            {
                IsBusy = false;
                IncomesList.Clear();
                TotalIncomes = IncomesList.Count;
                IncTitle = $"Today's Flow Ins";
                TotalAmount = 0;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    public void FilterGetIncOfSpecificMonth(DateTime specificDate)
    {
        try
        {
            IsBusy = true;
            double totalAmountFromList = 0;
            var incOfSpecificMonth = incomeService.OfflineIncomesList.FindAll(x => x.DateReceived.Month == specificDate.Month)
                .ToList();
            if (incOfSpecificMonth?.Count > 0)
            {
                IsBusy = false;
                IncomesList.Clear();
                foreach (IncomeModel inc in incOfSpecificMonth)
                {
                    IncomesList.Add(inc);
                    totalAmountFromList += inc.AmountReceived;
                }
                TotalAmount = totalAmountFromList;
                TotalIncomes = IncomesList.Count;
                IncTitle = $"Flow In For {DateTime.Now:MMM - yyyy}";
            }
            else
            {
                IsBusy = false;
                IncomesList.Clear();
                TotalIncomes = IncomesList.Count;
                IncTitle = $"Flow in For {DateTime.Now:MMM - yyyy}";
                TotalAmount = 0;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    public void GoToAddIncomePage()
    {
        if (ActiveUser is null)
        {
            Debug.WriteLine("Can't go because Active User is Null");
            Shell.Current.DisplayAlert("Wait", "Please Wait", "OK");
        }
        else
        {
            var navParam = new Dictionary<string, object>
            {
                {"SingleIncomeDetails", new IncomeModel{DateReceived = DateTime.Now} },
                {"PageTitle", new string("Add New Income")},
                {"ActiveUser",ActiveUser }
            };
            NavFunctions.FromManageIncToUpsertIncome(navParam);
        }
    }

    [RelayCommand]
    public void GoToEditIncomePage(IncomeModel income)
    {
        var navParam = new Dictionary<string, object>
        {
                {"SingleIncomeDetails", income},
                {"PageTitle", new string("Edit Income")},
                {"ActiveUser",ActiveUser }
        };
        NavFunctions.FromManageIncToUpsertIncome(navParam);
    }

    [RelayCommand]
    public async void DeleteIncomeBtn(IncomeModel income)
    {
        CancellationTokenSource cancellationTokenSource = new();
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        string text = "Income Deleted";
        var toast = Toast.Make(text, duration, fontSize);

        bool response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You want to delete?"));
        if (response)
        {
            var deleteResponse = await incomeService.DeleteIncomeAsync(income.Id);

            if (deleteResponse)
            {
                ActiveUser.TotalIncomeAmount -= income.AmountReceived;
                ActiveUser.PocketMoney -= income.AmountReceived;
                UserPockerMoney -= income.AmountReceived;

                await userService.UpdateUserAsync(ActiveUser);
                incomeService.OfflineIncomesList.Remove(income);
                IncomesList.Remove(income);

                await toast.Show(cancellationTokenSource.Token);
                FilterGetIncOfCurrentMonth();
            }
        }
    }

    [RelayCommand]
    public async Task PrintIncomesBtn()
    {
        if (IncomesList?.Count < 1)
        {
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("Cannot Save an Empty List to PDF"));
        }
        else
        {
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("Saved"));
        }
    }

    [RelayCommand]
    public async void ResetUserPocketMoney(double amount)
    {
        if (amount != 0)
        {
            ActiveUser.PocketMoney = amount;
            ActiveUser.DateTimeOfPocketMoneyUpdate = DateTime.UtcNow;
            userService.OfflineUser = ActiveUser;
            await userService.UpdateUserAsync(ActiveUser);

            CancellationTokenSource cancellationTokenSource = new();
            const ToastDuration duration = ToastDuration.Short;
            const double fontSize = 16;
            string text = "User Balance Updated!";
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token); //toast a notification about exp deletion

            PageLoaded();
        }
    }

    [RelayCommand]
    public  void ShowFilterPopUpPage()
    {
        //var filterOption = (string)await Shell.Current.ShowPopupAsync(new FilterOptionsPopUp("test"));
        //if (filterOption.Equals("Filter_All"))
        //{
        //    FilterGetAllIncomes();
        //}
        //else if (filterOption.Equals("Filter_Today"))
        //{
        //    FilterGetIncOfToday();
        //}
        //else if (filterOption.Equals("Filter_CurrMonth"))
        //{
        //    FilterGetIncOfCurrentMonth();
        //}
        //else
        //{
        //    //nothing was chosen
        //}

    }
}
