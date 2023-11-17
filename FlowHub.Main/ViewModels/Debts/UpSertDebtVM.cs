
using Plugin.Maui.CalendarStore;

namespace FlowHub.Main.ViewModels.Debts;

[QueryProperty(nameof(SingleDebtDetails), "SingleDebtDetails")]
public partial class UpSertDebtVM(IDebtRepository debtRepository, IUsersRepository usersRepository, ICalendarStore calendarStore) : ObservableObject
{
    readonly IDebtRepository debtRepo = debtRepository;
    readonly IUsersRepository userRepo = usersRepository;
    private readonly ICalendarStore calendarStoreRepo = calendarStore;
    [ObservableProperty]
    DebtModel singleDebtDetails;

    [ObservableProperty]
    InstallmentPayments singleInstallmentPayment;

    [ObservableProperty]
    string pageTitle;
    [ObservableProperty]
    bool hasDeadLine;
    [ObservableProperty]
    bool isBottomSheetOpened;
    [ObservableProperty]
    PopupResult thisPopUpResult;

    [ObservableProperty]
    bool closePopUp;
    bool isLent;
    bool isBorrow;
    public bool IsLent
    {
        get => isLent;
        set
        {
            if (SetProperty(ref isLent, value) && value)
            {
                SetProperty(ref isBorrow, false, nameof(IsBorrow));
            }
        }
    }
    public bool IsBorrow
    {
        get => isBorrow;
        set
        {
            if (SetProperty(ref isBorrow, value) && value)
            {
                SetProperty(ref isLent, false, nameof(IsLent));
            }
        }
    }

    public DebtType DebtType
    {
        get => IsLent ? DebtType.Lent : DebtType.Borrowed;
        set
        {
            IsLent = value == DebtType.Lent;
        }
    }

    [ObservableProperty]
    List<PersonOrOrganizationModel> listOfPersons;
    [ObservableProperty]
    List<string> listOfPersonsNames;
    public void PageLoaded()
    {
        DebtType = SingleDebtDetails.DebtType;
        
        IsLent = DebtType == DebtType.Lent;
        HasDeadLine = SingleDebtDetails.Deadline is not null;
        ListOfPersons = debtRepo.OfflineDebtList.Select(x => x.PersonOrOrganization )
            .Distinct()
            .ToList();
        ListOfPersonsNames = ListOfPersons.Select(x => x.Name)
            .Distinct()
            .ToList();
    }

    [RelayCommand]
    public async Task UpSertDebt()
    {
        if (string.IsNullOrEmpty(SingleDebtDetails.Notes) || string.IsNullOrWhiteSpace(SingleDebtDetails.Notes))
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Please add a Note!"));
            return;
        }
        CancellationTokenSource cts = new();
        const ToastDuration duration = ToastDuration.Short;

        SingleDebtDetails.UpdateDateTime = DateTime.UtcNow;
        SingleDebtDetails.PlatformModel = DeviceInfo.Current.Model;
        SingleDebtDetails.UserId = userRepo.OfflineUser.Id;
        SingleDebtDetails.DebtType = DebtType;
        if (HasDeadLine is false)
        {
            SingleDebtDetails.DisplayText = "Pending No Deadline Set";
        }
        else
        {
            var diff =  SingleDebtDetails.Deadline.Value.Date - DateTime.Now.Date;
            if (diff.TotalDays == 1)
            {
                SingleDebtDetails.DisplayText = "Pending Due in 1 Day";
            }
            else if(diff.TotalDays == 0)
            {
                SingleDebtDetails.DisplayText = "Pending Due Today!";
            }
            else if(diff.TotalDays > 1)
            {
                SingleDebtDetails.DisplayText = $"Pending Due in {diff.TotalDays} Days";
            }
            else if (diff.TotalDays < 0)
            {
                SingleDebtDetails.DisplayText = $"Pending Due PAST {diff.TotalDays} Days";
            }            
            
        }
        if (SingleDebtDetails.Id is not null)
        {
            await UpdateDebtAsync(14, cts, duration);
        }
        else
        {
            await AddDebtAsync(14, cts, duration);
        }

        ThisPopUpResult = PopupResult.OK;
        ClosePopUp = true;
    }

    [ObservableProperty]
    string selectedCalendarItem;
    private async Task AddDebtAsync(int fontSize, CancellationTokenSource cts, ToastDuration duration)
    {
        
        SingleDebtDetails.Id = Guid.NewGuid().ToString();
        SingleDebtDetails.AddedDateTime = DateTime.UtcNow;
        if (HasDeadLine is not true)
        {
            SingleDebtDetails.Deadline = null;
            SingleDebtDetails.DatePaidCompletely = null;
        }
        //this saves the debt to db and online
        if (!await debtRepo.AddDebtAsync(SingleDebtDetails))
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Failed to Add Flow Hold"));
            return;
        }

#if ANDROID
        if (HasDeadLine is true && SingleDebtDetails.Deadline is not null)
        {
            var calendarStatusRead = await CheckAndRequestReadCalendarPermission();
            if (calendarStatusRead != PermissionStatus.Granted)
            {
                return;
            }
            var calendarStatusWrite = await CheckAndRequestWriteCalendarPermission();
            if (calendarStatusWrite != PermissionStatus.Granted)
            {
                return;
            }

            var calendarsAccountsProfiles = await calendarStoreRepo.GetCalendars();


            if (calendarsAccountsProfiles is null || calendarsAccountsProfiles.Count() == 0)
            {
                await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("No Accounts found on this Device"));
                const string toastNotifMessageError = "Flow Hold Not Added";
                var toasts = Toast.Make(toastNotifMessageError, duration, fontSize);
                await toasts.Show(cts.Token);
                IsBottomSheetOpened = false;
                return;

            }

            var selectedProfile = await Shell.Current.DisplayActionSheet("Select Calendar Profie", "Cancel", null, calendarsAccountsProfiles.Select(x => x.Name).ToArray());
            if (selectedProfile == "Cancel")
            {
                return;
            }
            var calendarProfileID = calendarsAccountsProfiles
                .Where(x => x.Name == selectedProfile)
                .Select(x => x.Id)
                .FirstOrDefault();

            DateTimeOffset deadlineOffsetStart = new DateTimeOffset(SingleDebtDetails.Deadline.Value).AddHours(12);
            DateTimeOffset deadlineOffsetEnd = deadlineOffsetStart.AddMinutes(30);

            var eventID = await calendarStoreRepo.CreateEventWithReminder(calendarProfileID, "FlowHold Due Reminder !",
                $"{(SingleDebtDetails.DebtType == DebtType.Lent ? $"{SingleDebtDetails.PersonOrOrganization.Name} Owes You" : $"You Owe {SingleDebtDetails.PersonOrOrganization.Name}")} {SingleDebtDetails.Amount} {SingleDebtDetails.Currency} {Environment.NewLine}{SingleDebtDetails.PhoneAddress}",
                "FlowHub App", deadlineOffsetStart, deadlineOffsetEnd, 30);

            Debug.WriteLine("Event ID " + eventID);
        }

#endif
        const string toastNotifMessage = "Flow Hold Added";
        var toast = Toast.Make(toastNotifMessage, duration, fontSize);
        await toast.Show(cts.Token);
        IsBottomSheetOpened = false;
        
    }

    private async Task UpdateDebtAsync(int fontSize, CancellationTokenSource cts, ToastDuration duration)
    {
        if (!await debtRepo.UpdateDebtAsync(SingleDebtDetails))
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Failed to Update Flow Hold"));
            return;
        }

        //maybe i'll need to update user idk
        const string toastNotifMessage = "Flow Hold Updated";
        var toast = Toast.Make(toastNotifMessage, duration, fontSize);
        await toast.Show(cts.Token);
        IsBottomSheetOpened = false;
    }

    [RelayCommand]
    public void CancelBtn()
    {
        Debug.WriteLine("Action cancelled by user");
        ThisPopUpResult = PopupResult.Cancel;
        ClosePopUp = true;
    }

    [RelayCommand]
    public async Task UpSertInstallmentPayment()
    {
        if (SingleInstallmentPayment.Id is null)
        {
            AddInstallmentPayment();
            //ADD logic to update debt
            ThisPopUpResult = PopupResult.OK; 
            ClosePopUp = true;
        }
    }
    [RelayCommand]
    void CloseInstallmentsPopup()
    {
        ThisPopUpResult = PopupResult.Cancel;
        ClosePopUp = true;
    }
    private async void AddInstallmentPayment()
    {
        SingleInstallmentPayment.Id = Guid.NewGuid().ToString();

        if (SingleDebtDetails.PaymentAdvances is null )
        {
            SingleDebtDetails.PaymentAdvances = [SingleInstallmentPayment];
        }
        else
        {
            SingleDebtDetails.PaymentAdvances.Add(SingleInstallmentPayment);
        }
        SingleDebtDetails.Amount -= SingleInstallmentPayment.AmountPaid;
    }
    private async Task EditInstallmentPayment()
    {

    }
    private async Task DeleteInstallmentPayment()
    {

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

    public static async Task<PermissionStatus> CheckAndRequestReadCalendarPermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.CalendarRead>();

        if (status == PermissionStatus.Granted)
        {
            return status;
        }

        if (status == PermissionStatus.Denied)
        {
            status = await Permissions.RequestAsync<Permissions.CalendarRead>();
            return status;
        }

        status = await Permissions.RequestAsync<Permissions.CalendarRead>();

        return status;
    }

    /// <summary>
    /// CheckAndRequestWriteCalendarPermission
    /// </summary>
    /// <returns></returns>
    public static async Task<PermissionStatus> CheckAndRequestWriteCalendarPermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.CalendarWrite>();

        if (status == PermissionStatus.Granted)
        {
            return status;
        }

        if (status == PermissionStatus.Denied)
        {
            status = await Permissions.RequestAsync<Permissions.CalendarWrite>();
            return status;
        }

        status = await Permissions.RequestAsync<Permissions.CalendarWrite>();

        return status;
    }
}