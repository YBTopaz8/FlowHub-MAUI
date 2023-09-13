using FlowHub.Main.Utilities.BottomSheet;
using Org.BouncyCastle.Asn1.X509;

namespace FlowHub.Main.ViewModels.Debts;

public partial class ManageDebtsVM : ObservableObject
{
    private readonly IDebtRepository debtRepo;
    private readonly IUsersRepository usersRepo;

    public ManageDebtsVM(IDebtRepository debtRepository, IUsersRepository usersRepository)
    {
        debtRepo = debtRepository;
        usersRepo = usersRepository;
        debtRepo.OfflineDebtListChanged += HandleDebtsListUpdated;
    }
    //[ObservableProperty]
    //ObservableCollection<DebtModel> debtsList;
    [ObservableProperty]
    ObservableCollection<DebtModel> borrowedCompletedList;
    [ObservableProperty]
    ObservableCollection<DebtModel> lentCompletedList;
    [ObservableProperty]
    ObservableCollection<DebtModel> borrowedPendingList;
    [ObservableProperty]
    ObservableCollection<DebtModel> lentPendingList;
    

    [ObservableProperty]
    int totalDebts;

    [ObservableProperty]
    int totalLentCount;
    [ObservableProperty]
    double totalLentCompletedAmount;
    [ObservableProperty]
    double totalLentPendingAmount;

    [ObservableProperty]
    int totalBorrowedCount;
    [ObservableProperty]
    double totalBorrowedCompletedAmount;
    [ObservableProperty]
    double totalBorrowedPendingAmount;

    [ObservableProperty]
    UsersModel activeUser;
    [ObservableProperty]
    DebtModel singleDebtDetails;

    bool IsLoaded;

    [ObservableProperty]
    string userCurrency;

    [ObservableProperty]
    bool isShowCompletedChecked;
    [ObservableProperty]
    bool isShowPendingChecked;
    
    [ObservableProperty]
    bool isShowBorrowedChecked;
    [ObservableProperty]
    bool isShowLentChecked;

    [ObservableProperty]
    int totalPendingBorrowCount;
    [ObservableProperty]
    int totalPendingLentCount;
    
    [ObservableProperty]
    int totalCompletedBorrowCount;
    [ObservableProperty]
    int totalCompletedLentCount;

    
    public void PageLoaded()
    {
        try
        {
            if (!IsLoaded)
            {
                ApplyChanges();
                IsLoaded = true;
                ActiveUser = usersRepo.OfflineUser;
                UserCurrency = ActiveUser.UserCurrency;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception when loading all debts MESSAGE : {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task GoToAddDebtAsync()
    {
        var navParams = new Dictionary<string, object>()
        {
            {"SingleDebtDetails", new DebtModel()
                {
                  Amount = 0,
                  PersonOrOrganization = new PersonOrOrganizationModel(),
                  Currency = usersRepo.OfflineUser.UserCurrency
                } 
            },
            {"PageTitle", "Add Debt" }
        };
        await Shell.Current.GoToAsync(nameof(UpSertDebtPageM), true, navParams);
    }

    [RelayCommand]
    public async Task GoToEditDebtAsync(DebtModel debt)
    {
        var navParams = new Dictionary<string, object>()
        {
            {"SingleDebtDetails", debt },
            {"PageTitle", "Edit Debt" }
        };
        await Shell.Current.GoToAsync(nameof(UpSertDebtPageM), true, navParams);
    }
    public bool IsDeadlineSet
    {
        get => SingleDebtDetails.Deadline.HasValue;
    }

    ViewDebtBottomSheet debtBS;
    [RelayCommand]
    public async Task ViewDebtSheet(DebtModel debt)
    {
        SingleDebtDetails = debt;
        debtBS = new ViewDebtBottomSheet(this);
        await Drawer.Open(debtBS);
        

    }

    [ObservableProperty]
    List<string> listOfPeopleNames;
    public void ApplyChanges()
    {
        var filteredAndSortedDebts = debtRepo.OfflineDebtList
                        .Where(x => !x.IsDeleted)
                        .OrderByDescending(x => x.UpdateDateTime).ToList();

        ListOfPeopleNames = filteredAndSortedDebts
            .Select(x => x.PersonOrOrganization.Name)
            .Distinct()
            .ToList();
        RedoCountsAndAmountsCalculation(filteredAndSortedDebts);
    }

    private void RedoCountsAndAmountsCalculation(List<DebtModel> filteredAndSortedDebts)
    {
        
        BorrowedCompletedList = new ObservableCollection<DebtModel>(filteredAndSortedDebts
                    .Where(x => x.DebtType == DebtType.Borrowed && x.IsPaidCompletely));

        LentCompletedList = new ObservableCollection<DebtModel>(filteredAndSortedDebts
            .Where(x => x.DebtType == DebtType.Lent && x.IsPaidCompletely));

        BorrowedPendingList = new ObservableCollection<DebtModel>(filteredAndSortedDebts
            .Where(x => x.DebtType == DebtType.Borrowed && !x.IsPaidCompletely));
        LentPendingList = new ObservableCollection<DebtModel>(filteredAndSortedDebts
            .Where(x => x.DebtType == DebtType.Lent && !x.IsPaidCompletely));

        TotalPendingBorrowCount = BorrowedPendingList.Count;
        TotalCompletedBorrowCount = BorrowedCompletedList.Count;
        TotalPendingLentCount = LentPendingList.Count;
        TotalCompletedLentCount = LentCompletedList.Count;

        TotalBorrowedCompletedAmount = BorrowedCompletedList.Sum(x => x.Amount);
        TotalBorrowedPendingAmount = BorrowedPendingList.Sum(x => x.Amount);
        TotalLentCompletedAmount = LentCompletedList.Sum(x => x.Amount);
        TotalLentPendingAmount = LentPendingList.Sum(x => x.Amount);

        TotalBorrowedCount = TotalPendingBorrowCount + TotalCompletedBorrowCount;
        TotalLentCount = TotalPendingLentCount + TotalCompletedLentCount;

        

    }

    
    [RelayCommand]
    async Task SearchCommand(string query)
    {
        
        try
        {
            var ListOfDebts = debtRepo.OfflineDebtList
                .Where(
                        d=> d.PersonOrOrganization?.Name
                        .Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                .ToList();

            RedoCountsAndAmountsCalculation(ListOfDebts);
            
        }
        catch (TaskCanceledException ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    [RelayCommand]
    async Task DeleteDebtAsync(DebtModel debt)
    {
        CancellationTokenSource cancellationTokenSource = new();
        const ToastDuration duration = ToastDuration.Short;
        const double fontSize = 14;
        string text;
        try
        {
            //var response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Delete Flow Hold ?"));
            var response = await Shell.Current.DisplayAlert("Confirm Action", "Delete Flow Hold ?", "Yes", "No");
            if (response)
            {
                debt.PlatformModel = DeviceInfo.Current.Model;
                if (await debtRepo.DeleteDebtAsync(debt))
                {
                    text = "Flow Hold Deleted Successfully";
                }
                else
                {
                    text = "Flow Hold Deletion Failed";
                }
                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
                ApplyChanges();
                await Drawer.Close(debtBS);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception when deleting debt MESSAGE : {ex.Message}");
        }
    }

    [RelayCommand]
    async Task ToggleDebtCompletionStatus(DebtModel debt)
    {
        CancellationTokenSource cancellationTokenSource = new();
        const ToastDuration duration = ToastDuration.Short;
        const double fontSize = 14;
        string text;
        try
        {
            string message = debt.IsPaidCompletely ? "Mark as Pending" : "Mark as Completed";
            var response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert(message));
            if (response)
            {
                bool completedSwapper = !debt.IsPaidCompletely;
                
                if (debt.IsPaidCompletely) // to mark as pending
                {
                    debt.IsPaidCompletely = completedSwapper; // to unpaid completely
                    debt.DatePaidCompletely = null;
                    if (debt.Deadline.HasValue)
                    {
                        var diff = DateTime.Now.Date - debt.Deadline.Value.Date;
                        if (diff.TotalDays == 1)
                        {
                            debt.DisplayText = $"Due in {-diff.TotalDays} day";
                        }
                        if (diff.TotalDays > 1)
                        {
                            debt.DisplayText = $"Due past {diff.TotalDays} days!";
                        }
                        else if (diff.TotalDays < 0)
                        {
                            debt.DisplayText = $"Due in {-diff.TotalDays} days";
                        }
                        else
                        {
                            debt.DisplayText = "Due today";
                        }
                        
                    }
                    else
                    {
                        debt.DisplayText = "Pending No Deadline Set";
                    }
                    text = "Flow Hold Marked as Pending";
                }
                else
                {
                    debt.IsPaidCompletely = completedSwapper;
                    debt.DatePaidCompletely = DateTime.Now;

                    if (debt.Deadline.HasValue)
                    {
                        var DatePaidDiff = DateTime.Now.Date - debt.DatePaidCompletely?.Date;
                        if (DatePaidDiff.Value.TotalDays == 1)
                        {
                            debt.DisplayText = "Paid 1 day ago";
                        }
                        else if (DatePaidDiff.Value.TotalDays == 0)
                        {
                            debt.DisplayText = "Paid today";
                        }
                        else
                        {
                            debt.DisplayText = $"Paid {DatePaidDiff.Value.TotalDays} days ago";
                        }
                    }
                    else
                    {
                        debt.DisplayText = "Paid Today";
                    }
                    text = "Flow Hold Marked as Completed";
                }
                await debtRepo.UpdateDebtAsync(debt);

                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token); //toast a notification about exp deletion
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception when Marking as completed debt MESSAGE : {ex.Message}");
        }
    }

    [RelayCommand]
    void OpenPhoneDialer()
    {
        try
        {
            if (PhoneDialer.Default.IsSupported)
            {
                PhoneDialer.Default.Open(SingleDebtDetails.PersonOrOrganization.PhoneNumber);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception when opening phone dialer MESSAGE : {ex.Message}");
        }
    }

    private void HandleDebtsListUpdated()
    {
        try
        {
            ApplyChanges();
        }
        catch (Exception ex)
        {
            //await Shell.Current.DisplayAlert("Error debts", ex.Message, "OK");
            Debug.WriteLine("Error when added debts "+ ex.Message);
        }
    }
}