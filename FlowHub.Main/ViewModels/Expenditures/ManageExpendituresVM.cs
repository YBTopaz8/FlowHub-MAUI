﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using FlowHub.Main.Platforms.NavigationMethods;
using FlowHub.Main.PDF_Classes;
using CommunityToolkit.Maui.Views;
using FlowHub.Main.PopUpPages;
using UraniumUI.Extensions;


//This is the view model for the page that shows ALL expenditures
namespace FlowHub.Main.ViewModels.Expenditures;
public partial class ManageExpendituresVM : ObservableObject
{
    private readonly IExpendituresRepository expendituresService;
    private readonly IUsersRepository userService;

    private readonly ManageExpendituresNavs NavFunction = new();
    public ManageExpendituresVM(IExpendituresRepository expendituresRepository, IUsersRepository usersRepository)
    {
        expendituresService = expendituresRepository;
        userService = usersRepository;
    }
    public ObservableCollection<ExpendituresModel> ExpendituresList { get; set; } = new ();

    [ObservableProperty]
    private double totalAmount;

    [ObservableProperty]
    private int totalExpenditures;

    [ObservableProperty]
    private string userCurrency;

    [ObservableProperty]
    private double userPocketMoney;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string expTitle;

    private UsersModel ActiveUser = new();

    [ObservableProperty]
    private bool activ=false;

    [RelayCommand]
    public void Pageloaded()
    {        
                
        var user = userService.OfflineUser;
        ActiveUser = user;        

        UserPocketMoney = ActiveUser.PocketMoney;
        UserCurrency = ActiveUser.UserCurrency;

        FilterGetExpOfToday();
    }


    [RelayCommand]
    //THIS Function Shows all Expenditures for the current month
    public async void FilterGetExpListOfCurrentMonth()
    {
        try
        {
            IsBusy = true;
            double tot = 0;

            var expList = await expendituresService.GetAllExpendituresAsync();

            var ExpOfCurrentMonth = expList
                .FindAll(x => x.DateSpent.Month == DateTime.Today.Month)
                .ToList();
           
            if (ExpOfCurrentMonth?.Count > 0)
            {
                IsBusy = false;
                ExpendituresList.Clear();
                foreach (var exp in ExpOfCurrentMonth)
                {
                    ExpendituresList.Add(exp);
                    tot += exp.AmountSpent;
                }
                TotalAmount = tot;
                TotalExpenditures = ExpendituresList.Count;
                ExpTitle = $"Flow Outs For {DateTime.Now:MMM - yyyy}";
            }
            else
            {
                IsBusy=false;
                ExpendituresList.Clear();
                TotalExpenditures = ExpendituresList.Count;
                ExpTitle = $"Flow Outs For {DateTime.Now:MMM - yyyy}";
                TotalAmount = 0;
            }

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
            IsBusy= true;
            double tot = 0;
            var expList = await expendituresService.GetAllExpendituresAsync();

            if (expList?.Count > 0)
            {
                IsBusy = false;
                ExpendituresList.Clear();
                foreach (var exp in expList)
                {
                    ExpendituresList.Add(exp);
                    tot += exp.AmountSpent;
                }
                Debug.WriteLine(ExpendituresList.Count);
                TotalAmount = tot;
                TotalExpenditures = ExpendituresList.Count;
                ExpTitle = "All Flow Outs";
            }
            else
            {
                IsBusy= false;
                ExpendituresList.Clear();
                TotalAmount = 0;
                ExpTitle = "All Flow Outs";
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    //the Function below can be used to find exps for CURRENT DAY
    public async void FilterGetExpOfToday()
    {
        try
        {
            IsBusy = true;
            double tot = 0;
            var expList = await expendituresService.GetAllExpendituresAsync();

            var ExpOfToday = expList
                .FindAll(x => x.DateSpent.Day == DateTime.Today.Day)
               .ToList();

            if (ExpOfToday?.Count > 0)
            {
                IsBusy = false;
                ExpendituresList.Clear();
                foreach (var exp in ExpOfToday)
                {
                    ExpendituresList.Add(exp);
                    tot += exp.AmountSpent;
                }
                Debug.WriteLine(ExpendituresList.Count);
                TotalAmount = tot;
                TotalExpenditures = ExpOfToday.Count;
                ExpTitle = "Today's Flow Out";
            }
            else
            {
                IsBusy= false;
                ExpendituresList.Clear();
                TotalAmount = 0;
                TotalExpenditures = 0;
                ExpTitle = "Today's Flow Out";
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
            
        }
    }

    [RelayCommand]
    public void GoToAddExpenditurePage()
    {
        if (ActiveUser is null)
        {
            Debug.WriteLine("Can't go");
            Shell.Current.DisplayAlert("Wait", "Please wait", "Ok");
        }
        else
        {
            var navParam = new Dictionary<string, object>
            {
                { "SingleExpenditureDetails", new ExpendituresModel { DateSpent = DateTime.UtcNow } },
                { "PageTitle", new string("Add New Expenditure") },
                { "ActiveUser", ActiveUser }
            };

            NavFunction.FromManageExpToUpsertExpenditures(navParam);
        }
    }

    [RelayCommand]
    public void GoToEditExpenditurePage(ExpendituresModel expenditure)
    {
        Debug.WriteLine("Entered edit");
        var navParam = new Dictionary<string, object>
        {
            { "SingleExpenditureDetails", expenditure },
            { "PageTitle", new string("Edit Expenditure") },
            { "ActiveUser", ActiveUser }
        };

        NavFunction.FromManageExpToUpsertExpenditures(navParam);
    }
    [RelayCommand]
    public async void DeleteExpenditureBtn(ExpendituresModel expenditure)
    {
        CancellationTokenSource cancellationTokenSource = new();
        ToastDuration duration = ToastDuration.Short;
        double fontSize = 14;
        string text = "Expenditure Deleted";
        var toast = Toast.Make(text, duration, fontSize);

        bool response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You want to delete?"));
        if (response)
        {
            var deleteResponse = await expendituresService.DeleteExpenditureAsync(expenditure); //delete the expenditure from db
                    
            ExpendituresList.Remove(expenditure);
            if (deleteResponse)
            {
                ActiveUser.PocketMoney += expenditure.AmountSpent;
                UserPocketMoney += expenditure.AmountSpent;
                await userService.UpdateUserAsync(ActiveUser);

                await toast.Show(cancellationTokenSource.Token); //toast a notification about exp deletion
                FilterGetExpListOfCurrentMonth(); //refresh the collectionview
            }
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
        var filterOption = (string)await Shell.Current.ShowPopupAsync(new FilterOptionsPopUp());
        if (filterOption.Equals("Filter_All"))
        {
            FilterGetAllExp();
        }
        else if (filterOption.Equals("Filter_Today"))
        {
            FilterGetExpOfToday();
        }
        else if (filterOption.Equals("Filter_CurrMonth"))
        {
            FilterGetExpListOfCurrentMonth();
        }
        else
        {
            //nothing was chosen
        }
    }
        [RelayCommand]
    public async void SyncExpTest()
    {
        //  await expendituresService.GetAllExpFromOnlineAsync(ActiveUser.Id);
        string newText= (string)await Shell.Current.ShowPopupAsync(new InputPopUpPage(isTextInput:true, optionalTitleText:"Enter New Name"));
        //Debug.WriteLine(newText);
    }
}
