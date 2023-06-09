using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Main.PDF_Classes;
using FlowHub.Main.Platforms.NavigationMethods;
using FlowHub.Main.PopUpPages;
using FlowHub.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

//This is the view model for the page that shows ALL expenditures
namespace FlowHub.Main.ViewModels.Expenditures;
public partial class ManageExpendituresVM : ObservableObject
{
    public readonly IExpendituresRepository expendituresService;
    private readonly IUsersRepository userService;

    private readonly ManageExpendituresNavs NavFunction = new();
    public ManageExpendituresVM(IExpendituresRepository expendituresRepository, IUsersRepository usersRepository)
    {
        expendituresService = expendituresRepository;
        userService = usersRepository;
    }
    
    [ObservableProperty]
    ObservableCollection<ExpendituresModel> expendituresList;

    [ObservableProperty]
    private double totalAmount;

    [ObservableProperty]
    private int totalExpenditures;

    [ObservableProperty]
    private string userCurrency;

    [ObservableProperty]
    private double userPocketMoney;

    [ObservableProperty]
    private bool isBusy = true;

    [ObservableProperty]
    private string expTitle;

    private UsersModel ActiveUser = new();

    [ObservableProperty]
    private bool activ;

    [ObservableProperty]
    private bool showDayFilter = false;

    [ObservableProperty]
    private bool showStatisticBtn = false;

    [ObservableProperty]
    private int dayFilterMonth ;

    [ObservableProperty]
    private int dayFilterYear;

    [ObservableProperty]
    private int dayFilterDay;

    [ObservableProperty]
    private string filterTitle;

    [ObservableProperty]
    private bool showClearDayButton = false;

    [ObservableProperty]
    private bool isExpanderExpanded = false;

    [ObservableProperty]
    private bool isSyncing = false;

    private string filterOption;
    private int GlobalSortNamePosition = 1;

    private string monthName;
    [RelayCommand]
    public async void PageloadedAsync()
    {
        DayFilterYear = DateTime.UtcNow.Year;
        DayFilterMonth = DateTime.UtcNow.Month;

        UsersModel user = userService.OfflineUser;
        ActiveUser = user;

        UserPocketMoney = ActiveUser.PocketMoney;
        UserCurrency = ActiveUser.UserCurrency;
        await expendituresService.GetAllExpendituresAsync();
        filterOption = "Filter_Curr_Month";
        //FilterGetExpOfToday(GlobalSortNamePosition);        
        FilterGetAllExp();
      //  FilterGetAllExp();
    }

    [RelayCommand]
    public void Sorting(int SortNamePosition)
    {
        IsBusy= true;
        GlobalSortNamePosition = SortNamePosition;

        var expList = new List<ExpendituresModel>();
        switch (SortNamePosition)
        {
            case 0:
                expList = ExpendituresList.OrderBy(x => x.DateSpent).ToList();
                FilterTitle = "Date Spent Ascending";
                break;
            case 1:
                expList = ExpendituresList.OrderByDescending(x => x.DateSpent).ToList();
                FilterTitle = "Date Spent Descending";
                break;
            case 2:
                expList = ExpendituresList.OrderBy(x => x.AmountSpent).ToList();
                FilterTitle = "Amount Spent Ascending";
                break;
            case 3:
                expList = ExpendituresList.OrderByDescending(x => x.AmountSpent).ToList();
                FilterTitle = "Amount Spent Descending";
                break;
            default:
                break;
        }

        ExpendituresList.Clear();

        foreach (ExpendituresModel exp in expList)
        {
            ExpendituresList.Add(exp);
        }
        IsBusy = false;
    }

    [RelayCommand]
    //THIS Function Shows all Expenditures for the current month
    public void FilterExpListOfCurrentMonth()
    {
        // expendituresService.OfflineExpendituresList is ALREADY loaded since it was filled in the HomePageVM
        try
        {
            ShowDayFilter = true;
            ShowClearDayButton = false;
            DayFilterMonth = DateTime.UtcNow.Month;
            DayFilterYear = DateTime.UtcNow.Year;
            filterOption = "Filter_Curr_Month";
            List<ExpendituresModel> expOfCurrentMonth = new();

            expOfCurrentMonth = expendituresService.OfflineExpendituresList
                            .FindAll(x => x.DateSpent.Month == DateTime.Today.Month)
                            .OrderByDescending(x => x.DateSpent)
                            .ToList();
            FilterTitle = "Date Spent Descending";

            //IsBusy = true;

            ExpendituresList = new ObservableCollection<ExpendituresModel>(expOfCurrentMonth);
            //ExpendituresList.Clear();

            ExpTitle = $"Flow Outs For {DateTime.Now:MMM - yyyy}";

            TotalAmount = ExpendituresList.Count > 0 ? ExpendituresList.Count : 0;
            IsBusy = false;

            //if (expOfCurrentMonth.Count > 0)
            //{
            //    IsBusy = false;
            //    for (int i = 0; i < expOfCurrentMonth.Count; i++)
            //    {
            //        ExpendituresList.Add(expOfCurrentMonth[i]);
            //        tot += expOfCurrentMonth[i].AmountSpent;
            //    }

            //    TotalAmount = tot;
            //    TotalExpenditures = ExpendituresList.Count;
            //    ExpTitle = $"Flow Outs For {DateTime.Now:MMM - yyyy}";
            //}
            //else
            //{
            //    IsBusy=false;
            //    TotalExpenditures = ExpendituresList.Count;
            //    ExpTitle = $"Flow Outs For {DateTime.Now:MMM - yyyy}";
            //    TotalAmount = 0;
            //}
            ShowStatisticBtn = expOfCurrentMonth.Count >= 3;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    public void FilterExpListOfSpecificMonth()
    {
        try
        {
            ShowDayFilter = true;
            ShowClearDayButton = false;
            DayFilterDay = 0;

            filterOption = "Filter_Spec_Month";
            List<ExpendituresModel> expOfSpecMonth = new();

            expOfSpecMonth = expendituresService.OfflineExpendituresList
                            .FindAll(x => x.DateSpent.Month == DayFilterMonth && x.DateSpent.Year == DayFilterYear)
                            .OrderByDescending(x => x.DateSpent)
                            .ToList();
            IsBusy = true;
            double tot = 0;

            if (expOfSpecMonth.Count > 0)
            {
                IsBusy = false;
                ExpendituresList.Clear();
                for (int i = 0; i < expOfSpecMonth.Count; i++)
                {
                    ExpendituresList.Add(expOfSpecMonth[i]);
                    tot += expOfSpecMonth[i].AmountSpent;
                }

                TotalAmount = tot;
                TotalExpenditures = ExpendituresList.Count;
                ExpTitle = $"Flow Outs For {monthName} - {DayFilterYear}";
            }
            else
            {
                IsBusy=false;
                ExpendituresList.Clear();
                TotalExpenditures = ExpendituresList.Count;
                ExpTitle = $"Flow Outs For {monthName} - {DayFilterYear}";
                TotalAmount = 0;
            }

            ShowStatisticBtn = expOfSpecMonth.Count >= 3;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    //Function to show very single expenditure from DB
    public async void FilterGetAllExp()
    {
        try
        {
            ShowDayFilter = false;
            filterOption = "Filter_All";
            List<ExpendituresModel> expList = new ();

            expList = expendituresService.OfflineExpendituresList.OrderByDescending(x => x.DateSpent).ToList();
            FilterTitle = "Date Spent Descending";

            var tempList = await Task.Run(async () => new ObservableCollection<ExpendituresModel>(expList));
            ExpendituresList = tempList;
            
            
            TotalAmount = ExpendituresList.Sum(x => x.AmountSpent);
            TotalExpenditures = ExpendituresList.Count;

            IsBusy = false;
                //if (ActiveUser.TotalExpendituresAmount == 0)
                //{
                //    ActiveUser.TotalExpendituresAmount = tot;
                //    await userService.UpdateUserAsync(ActiveUser);
                //}
            
                ExpTitle = "All Flow Outs";
            
            ShowStatisticBtn = expList.Count >= 3;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    //the Function below can be used to find exps for CURRENT DAY
    public void FilterGetExpOfToday()
    {
        try
        {

            Debug.WriteLine("entered today");
            ShowDayFilter = false;
            filterOption = "Filter_Today";
            List<ExpendituresModel> expOfToday = new();

            expOfToday = expendituresService.OfflineExpendituresList
                        .FindAll(x => x.DateSpent.Date == DateTime.Today.Date)
                        .OrderByDescending(x => x.DateSpent)
                        .ToList();
            FilterTitle = "Date Spent Descending";

            IsBusy = true;
            double tot = 0;

            if (expOfToday.Count > 0)
            {
                IsBusy = false;
                ExpendituresList.Clear();
                foreach (var exp in expOfToday)
                {
                    ExpendituresList.Add(exp);
                    tot += exp.AmountSpent;
                }

                TotalAmount = tot;
                TotalExpenditures = expOfToday.Count;
                ExpTitle = "Today's Flow Out";
            }
            else
            {
                IsBusy = false;
                ExpendituresList.Clear();
                TotalAmount = 0;
                TotalExpenditures = 0;
                ExpTitle = "Today's Flow Out";
            }
            ShowStatisticBtn = expOfToday.Count >= 3;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    public void FilterGetExpOfSpecificDayMonth()
    {
        try
        {
            ShowClearDayButton = true;
            ShowDayFilter = true;
            IsExpanderExpanded = false;

            filterOption = "Filter_Spec_Day_Month";
            DateTime specificDate = new(DayFilterYear, DayFilterMonth, DayFilterDay);
            List<ExpendituresModel> expOfSpecDayInMonth = new();

            expOfSpecDayInMonth = expendituresService.OfflineExpendituresList
                            .FindAll(x => x.DateSpent.Date == specificDate)
                            .OrderByDescending(x => x.DateSpent)
                            .ToList();
            FilterTitle = "Date Spent Descending";

            IsBusy = true;
            double tot = 0;
            if (expOfSpecDayInMonth.Count > 0)
            {
                IsBusy = false;
                ExpendituresList.Clear();
                for (int i = 0; i < expOfSpecDayInMonth.Count; i++)
                {
                    ExpendituresList.Add(expOfSpecDayInMonth[i]);
                    tot += expOfSpecDayInMonth[i].AmountSpent;
                }

                TotalAmount = tot;
                TotalExpenditures = ExpendituresList.Count;
                ExpTitle = $"Flow Outs For {specificDate:dd, MMM yyyy}";
            }
            else
            {
                IsBusy = false;
                ExpendituresList.Clear();
                TotalExpenditures = ExpendituresList.Count;
                ExpTitle = $"Flow Outs For {specificDate:dd, MMM yyyy}";
                TotalAmount = 0;
            }
            ShowStatisticBtn = expOfSpecDayInMonth.Count >= 3;
            GlobalSortNamePosition = 1;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    [RelayCommand]
    public async void GoToAddExpenditurePage()
    {
        if (ActiveUser is null)
        {
            Debug.WriteLine("Can't go");
           await Shell.Current.DisplayAlert("Wait", "Cannot go", "Ok");
        }
        else
        {
            Dictionary<string, object> navParam = new()
            {
                { "SingleExpenditureDetails", new ExpendituresModel { DateSpent = DateTime.Now } },
                { "PageTitle", new string("Add New Flow Out") },
                { "IsAdd", true },
                { "ActiveUser", ActiveUser }
            };

            NavFunction.FromManageExpToUpsertExpenditures(navParam);
        }
    }

    [RelayCommand]
    public void GoToEditExpenditurePage(ExpendituresModel expenditure)
    {
        var navParam = new Dictionary<string, object>
        {
            { "SingleExpenditureDetails", expenditure },
            { "PageTitle", new string("Edit Flow Out") },
            { "IsAdd", false },
            { "ActiveUser", ActiveUser }
        };

        NavFunction.FromManageExpToUpsertExpenditures(navParam);
    }

    [RelayCommand]
    public void GoToSpecificStatsPage()
    {
        int monthNumb = DayFilterMonth;
        int YearNumb = DayFilterYear;

        var navParam = new Dictionary<string, object>
        {
            { "MonthNumber",  monthNumb },
            { "YearNumber",  YearNumb},
            { "ExpendituresList", ExpendituresList }
        };
        switch (filterOption)
        {
            case "Filter_All":
                navParam.Add("PageTitle", new string("Statistics of All Flow Outs"));
                break;
            case "Filter_Today":
                navParam.Add("PageTitle", new string("Statistics of Today's Flow Outs"));
                break;
            case "Filter_Curr_Month":
                navParam.Add("PageTitle", new string("Statistics of this Month's Flow Outs"));
                break;
            case "Filter_Spec_Month":
                navParam.Add("PageTitle", new string($"Statistics of Flow Outs for {monthNumb} - {YearNumb}"));
                break;
            case "Filter_Spec_Day_Month":
                navParam.Add("PageTitle", new string($"Statistics of date {DayFilterDay}-{DayFilterMonth}-{DayFilterYear}"));
                break;
            default:
                //Debug.WriteLine("Cancelled");
                break;
        }

        NavFunction.FromManageExpToSingleMonthStats(navParam);
    }

    [RelayCommand]
    public async void DeleteExpenditureBtn(ExpendituresModel expenditure)
    {
        CancellationTokenSource cancellationTokenSource = new();
        const ToastDuration duration = ToastDuration.Short;
        const double fontSize = 14;
        string text;
        bool response = (bool)(await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You want to delete?")))!;
        if (response)
        {
            IsBusy = true;
            var deleteResponse = await expendituresService.DeleteExpenditureAsync(expenditure.Id); //delete the expenditure from db

            if (deleteResponse)
            {
                text = "Expenditure Deleted";
                expendituresService.OfflineExpendituresList.Remove(expenditure);
                ExpendituresList.Remove(expenditure);

                ActiveUser.TotalExpendituresAmount -= expenditure.AmountSpent;
                ActiveUser.PocketMoney += expenditure.AmountSpent;
                UserPocketMoney += expenditure.AmountSpent;
                await userService.UpdateUserAsync(ActiveUser);
            }
            else
            {
                 text = "Expenditure Not Deleted";
            }
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token); //toast a notification about exp deletion
            Sorting(GlobalSortNamePosition);
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task PrintExpendituresBtn()
    {
        Activ = true;
        if (ExpendituresList?.Count < 1)
        {
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("Cannot save an Empty list to PDF"));
        }
        else
        {
            PrintExpenditures prtt = new();
            prtt.SaveExpenditureToPDF(ExpendituresList, UserCurrency);
        }
    }
    [RelayCommand]
    public async void ShowFilterPopUpPage()
    {
        List<string> FilterResult = new();

        FilterResult = (List<string>)await Shell.Current.ShowPopupAsync(new FilterOptionsPopUp(filterOption, DayFilterMonth,DayFilterYear));
        GlobalSortNamePosition = 1;
        if (FilterResult.Count == 1)
        {
            filterOption = FilterResult[0];
            switch (filterOption)
            {
                case "Filter_All":
                    FilterGetAllExp();
                    break;
                case "Filter_Today":
                    FilterGetExpOfToday();
                    break;
                case "Filter_Curr_Month":
                    FilterExpListOfCurrentMonth();
                    break;
                default:
                    //Debug.WriteLine("Cancelled");
                    break;
            }
        }
        else
        {
            filterOption = FilterResult[0];
            DayFilterMonth= int.Parse(FilterResult[1]);
            DayFilterYear = int.Parse(FilterResult[2]);
            monthName= FilterResult[3];
            FilterExpListOfSpecificMonth();
        }
    }

    [RelayCommand]
    public async void SyncExpTest()
    {
        bool response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do you want to Sync data?"));
        if (response)
        {
            IsBusy = true;
            if (await expendituresService.SynchronizeExpendituresAsync(ActiveUser.Email, ActiveUser.Password))
            {
                PageloadedAsync();
                IsBusy = false;
                CancellationTokenSource cancellationTokenSource = new();
                const ToastDuration duration = ToastDuration.Short;
                const double fontSize = 16;
                string text = "All Synchronized!";
                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token); //toast a notification about Sync Success0!
            }
        }
    }

    [RelayCommand]
    public async void DropCollection()
    {
        await expendituresService.DropExpendituresCollection();
    }
}
