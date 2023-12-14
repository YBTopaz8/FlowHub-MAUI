
using Plugin.Maui.CalendarStore;

namespace FlowHub.Main.ViewModels.Debts;

public partial class ManageDebtsVM : ObservableObject
{
    private readonly IDebtRepository debtRepo;
    private readonly IUsersRepository usersRepo;
    private readonly UpSertDebtVM upSertDebtVM;

    public ManageDebtsVM(IDebtRepository debtRepository, IUsersRepository usersRepository, 
        UpSertDebtVM upSertDebtViewModel )
    {
        debtRepo = debtRepository;
        usersRepo = usersRepository;
        upSertDebtVM = upSertDebtViewModel;
        debtRepo.OfflineDebtListChanged += HandleDebtsListUpdated;
    }
    [ObservableProperty]
    ObservableCollection<DebtModel> debtsList;
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

    [ObservableProperty]
    string titleText;

    [ObservableProperty]
    bool showSingleSebt;
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
                SingleDebtDetails = new DebtModel()
                { 
                    Amount = 0, 
                    PersonOrOrganization = new()
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception when loading all debts MESSAGE : {ex.Message}");
        }
    }


    [RelayCommand]
    public async Task ShowDebtDetails(DebtModel debt)
    {
#if ANDROID
        SingleDebtDetails = debt;

        await Shell.Current.GoToAsync(nameof(SingleDebtDetailsPageM), true);
#elif WINDOWS
        ShowSingleSebt = true;
        SingleDebtDetails = debt;
        RefreshTitleText();
#endif

    }

    public void RefreshTitleText()
    {
        TitleText = SingleDebtDetails.DebtType == DebtType.Lent
                    ? $"{SingleDebtDetails.PersonOrOrganization.Name} Owes You {SingleDebtDetails.Amount} {SingleDebtDetails.Currency}"
                    : $"You Owe {SingleDebtDetails.PersonOrOrganization.Name}, {SingleDebtDetails.Amount} {SingleDebtDetails.Currency}";
    }

    [ObservableProperty]
    List<string> listOfPeopleNames;
    public void ApplyChanges()
    {
        var filteredAndSortedDebts = debtRepo.OfflineDebtList
                        .Where(x => !x.IsDeleted)
                        .OrderByDescending(x => x.UpdateDateTime)
                        .OrderBy(x => x.IsPaidCompletely)
                        .Distinct()
                        .ToList();
        DebtsList = filteredAndSortedDebts.ToObservableCollection();
        ListOfPeopleNames = filteredAndSortedDebts
            .Select(x => x.PersonOrOrganization.Name)
            .Distinct()
            .ToList();
        RedoCountsAndAmountsCalculation(filteredAndSortedDebts);
    }

    private void RedoCountsAndAmountsCalculation(List<DebtModel> filteredAndSortedDebts)
    {

        BorrowedCompletedList = new ObservableCollection<DebtModel>(filteredAndSortedDebts
            .Where(x => x.DebtType == DebtType.Borrowed && x.IsPaidCompletely)
            .OrderBy(x => x.AddedDateTime));

        LentCompletedList = new ObservableCollection<DebtModel>(filteredAndSortedDebts
            .Where(x => x.DebtType == DebtType.Lent && x.IsPaidCompletely)
            .OrderBy(x => x.AddedDateTime));

        BorrowedPendingList = new ObservableCollection<DebtModel>(filteredAndSortedDebts
            .Where(x => x.DebtType == DebtType.Borrowed && !x.IsPaidCompletely)
            .OrderBy(x => x.AddedDateTime));
        LentPendingList = new ObservableCollection<DebtModel>(filteredAndSortedDebts
            .Where(x => x.DebtType == DebtType.Lent && !x.IsPaidCompletely)
            .OrderBy(x => x.AddedDateTime));

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
    public async Task ShowAddDebtPopUp()
    {
        if (ActiveUser is null)
        {
            Debug.WriteLine("Can't Open Add Debt PopUp because User is null");
            await Shell.Current.DisplayAlert("Wait", "Cannot go", "Ok");
        }
        else
        {

            var newDebt = new DebtModel
            {
                Amount = 1,
                PersonOrOrganization = new PersonOrOrganizationModel(),
                Currency = ActiveUser.UserCurrency
            };
            upSertDebtVM.SingleDebtDetails = newDebt;
            upSertDebtVM.IsLent = true;
            await AddEditDebt();
        }
    }

    [RelayCommand]
    public async Task ShowEditDebtPopUp(DebtModel debt)
    {
        upSertDebtVM.SingleDebtDetails = debt;
        upSertDebtVM.HasDeadLine = debt.Deadline is not null;
        upSertDebtVM.IsLent = debt.DebtType == DebtType.Lent;
        await AddEditDebt();
    }
    private async Task AddEditDebt()
    {
        
        var result = (PopUpCloseResult)await Shell.Current.ShowPopupAsync(new UpSertDebtPopUp(upSertDebtVM));
        if (result.Result == PopupResult.OK)
        {
            Debug.WriteLine("Popup Closed OK");
        }
    }

    [RelayCommand]
    void SearchBar(string query)
    {        
        try
        {
            var ListOfDebts = debtRepo.OfflineDebtList
            .Where(d =>d.PersonOrOrganization?.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            .Where(d=>!d.IsDeleted)
            .ToList();

            RedoCountsAndAmountsCalculation(ListOfDebts);
            
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FLOW HOLD EXCEPTION : {ex.Message}");
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
                //await Drawer.Close(debtBS);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception when deleting debt MESSAGE : {ex.Message}");
        }
    }

    [RelayCommand]
    async Task UpSertInstallmentPaymentPopUp(InstallmentPayments selectedInstallment = null)
    {
        upSertDebtVM.SingleDebtDetails = SingleDebtDetails;
        upSertDebtVM.SingleInstallmentPayment = selectedInstallment is null ? new() { AmountPaid = 0, DatePaid = DateTime.Now } : selectedInstallment;
        upSertDebtVM.selectedInstallmentInitialAmount = selectedInstallment is null ? 0 : selectedInstallment.AmountPaid;
        
        var result = (PopUpCloseResult)await Shell.Current.ShowPopupAsync(new UpSertInstallmentPayment(upSertDebtVM));
        {
            RefreshTitleText();
            Debug.WriteLine($"Installments Popup Closed {result.Result}");
        }
    }
    [RelayCommand]
    async Task DeleteInstallment(InstallmentPayments selectedInstallment)
    {
        var deletedResult = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Confirm Delete?"));
        if (deletedResult)
        {
            upSertDebtVM.SingleDebtDetails = SingleDebtDetails;
            upSertDebtVM.SingleInstallmentPayment = selectedInstallment;

            await upSertDebtVM.DeleteInstallmentPayment(selectedInstallment);
            RefreshTitleText();           

        }
    }

    [RelayCommand]
    async Task ToggleDebtCompletionStatus(object s)
    {
        if (s.GetType() == typeof(DebtModel))
        {
            var debt = (DebtModel)s;
            CancellationTokenSource cancellationTokenSource = new();
            const ToastDuration duration = ToastDuration.Short;
            const double fontSize = 14;
            string text;
            try
            {
                string message = debt.IsPaidCompletely ? "Mark as Completed" : "Mark as Pending" ;
                var response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert(message));
                if (response)
                {
                    bool completedSwapper = !debt.IsPaidCompletely;

                    if (!debt.IsPaidCompletely) // to mark as pending
                    {
#if ANDROID
                        debt.IsPaidCompletely = completedSwapper; // to unpaid completely  
#endif
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
#if ANDROID
                        debt.IsPaidCompletely = completedSwapper; 
#endif
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
                    ApplyChanges();
                }
                else
                {
                    debt.IsPaidCompletely = !debt.IsPaidCompletely;
                    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception when Marking as completed debt MESSAGE : {ex.Message}");
            }
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