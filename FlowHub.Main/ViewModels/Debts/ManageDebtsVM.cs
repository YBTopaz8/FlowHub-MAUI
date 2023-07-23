using FlowHub.Main.Utilities.BottomSheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    [ObservableProperty]
    ObservableCollection<DebtModel> debtsList;
    [ObservableProperty]
    int totalDebts;
    [ObservableProperty]
    int totalLent;
    [ObservableProperty]
    int totalBorrowed;
    [ObservableProperty]
    UsersModel activeUser;
    [ObservableProperty]
    DebtModel singleDebtDetails;

    bool IsLoaded;
    public void PageLoaded()
    {
        try
        {
            if (!IsLoaded)
            {
                ApplyChanges();
                IsLoaded = true;
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
            } },
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
        //_ = await Drawer.Open(new ViewDebtBottomSheet(this));

    }
    private void ApplyChanges()
    {
        List<DebtModel> debtList = new();
        debtList = debtRepo.OfflineDebtList
                    .Where(x => !x.IsDeleted)
                    .OrderByDescending(x => x.UpdateDateTime)
                    .ToList();
        DebtsList?.Clear();
        DebtsList = new ObservableCollection<DebtModel>(debtList);

        TotalBorrowed = debtList.Count(x => x.DebtType == DebtType.Borrowed);//.Sum(x => x.Amount);
        TotalLent = debtList.Count(x => x.DebtType == DebtType.Lent);//.Sum(x => x.Amount);
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
                if (!debt.IsPaidCompletely)
                {
                    debt.IsPaidCompletely = true;
                    debt.DatePaidCompletely = DateTime.Now;
                    text = "Flow Hold Marked as Completed";
                }
                else
                {
                    debt.IsPaidCompletely = false;
                    debt.Deadline = null;
                    text = "Flow Hold Marked as Pending";
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

    private async void HandleDebtsListUpdated()
    {
        try
        {
            ApplyChanges();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error debts", ex.Message, "OK");
        }
    }
}