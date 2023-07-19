
using CommunityToolkit.Maui.Core;

namespace FlowHub.Main.ViewModels.Debts;

[QueryProperty(nameof(SingleDebtDetails), "SingleDebtDetails")]
public partial class UpSertDebtVM : ObservableObject
{
    readonly IDebtRepository debtRepo;
    readonly IUsersRepository userRepo;
    public UpSertDebtVM(IDebtRepository debtRepository, IUsersRepository usersRepository)
    {
        debtRepo = debtRepository;
        userRepo = usersRepository;
    }

    [ObservableProperty]
    DebtModel singleDebtDetails;

    [ObservableProperty]
    string pageTitle;
    [ObservableProperty]
    bool hasDeadLine;

    bool isLent;
    bool isBorrow;

    public bool IsLent
    {
        get => isLent;
        set
        {
            if (SetProperty(ref isLent, value) && value)
            {
                isBorrow = false;
            }
            OnPropertyChanged(nameof(isLent));
        }
    }
    public bool IsBorrow
    {
        get => isBorrow;
        set
        {
            if (SetProperty(ref isBorrow, value) && value)
            {
                IsLent = false;
            }
            OnPropertyChanged(nameof(isBorrow));
        }
    }

    public DebtType DebtType
    {
        get => isLent ? DebtType.Lent : DebtType.Borrowed;
        set
        {
            if (value == DebtType.Lent)
            {
                IsLent = true;
            }
            else
            {
                IsBorrow = true;
            }
        }
    }

    public void PageLoaded()
    {
        DebtType = SingleDebtDetails.DebtType;
        HasDeadLine = SingleDebtDetails.Deadline is not null;
    }

    [RelayCommand]
    public async Task UpSertDebt()
    {
        CancellationTokenSource cts = new();
        const ToastDuration duration = ToastDuration.Short;

        SingleDebtDetails.UpdateDateTime = DateTime.UtcNow;
        SingleDebtDetails.PlatformModel = DeviceInfo.Current.Model;
        SingleDebtDetails.UserId = userRepo.OfflineUser.Id;
        SingleDebtDetails.DebtType = DebtType;

        if (SingleDebtDetails.Id is not null)
        {
            await UpdateDebtAsync(14, cts, duration);
        }
        else
        {
            await AddDebtAsync(14, cts, duration);
        }
    }

    private async Task AddDebtAsync(int fontSize, CancellationTokenSource cts, ToastDuration duration)
    {
        SingleDebtDetails.Id = Guid.NewGuid().ToString();
        SingleDebtDetails.AddedDateTime = DateTime.UtcNow;

        if (!await debtRepo.AddDebtAsync(SingleDebtDetails))
        {
            return;
        }
        const string toastNotifMessage = "Flow Hold Added";
        var toast = Toast.Make(toastNotifMessage, duration, fontSize);
        await toast.Show(cts.Token);
        await ManageExpendituresNavs.ReturnOnce();
    }

    private async Task UpdateDebtAsync(int fontSize, CancellationTokenSource cts, ToastDuration duration)
    {
        if (!await debtRepo.UpdateDebtAsync(SingleDebtDetails))
        {
            return;
        }

        //maybe i'll need to update user idk
        const string toastNotifMessage = "Flow Hold Updated";
        var toast = Toast.Make(toastNotifMessage, duration, fontSize);
        await toast.Show(cts.Token);
        await ManageExpendituresNavs.ReturnOnce();
    }

    [RelayCommand]
    async Task ContactDetailsPicker()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.ContactsRead>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.ContactsRead>();
            }

            var PickedContact = await Contacts.Default.PickContactAsync();
            if (PickedContact is null)
            {
                Debug.WriteLine("Contact not picked");
            }
            SingleDebtDetails.PersonOrOrganization = new()
            {
                Name = PickedContact.DisplayName,
                PhoneNumber = PickedContact.Phones.FirstOrDefault()?.PhoneNumber,
                Email = PickedContact.Emails.FirstOrDefault()?.EmailAddress
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Permission denied " + ex.Message);
        }
    }
}