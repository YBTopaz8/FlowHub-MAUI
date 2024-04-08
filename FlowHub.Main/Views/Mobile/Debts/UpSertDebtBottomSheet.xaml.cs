

using static Android.Icu.Text.CaseMap;

namespace FlowHub.Main.Views.Mobile.Debts;

public partial class UpSertDebtBottomSheet : BottomSheetView
{
    readonly UpSertDebtVM viewModel;
    public UpSertDebtBottomSheet(UpSertDebtVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
        
   
	}

   

    private void AmountTextField_Focused(object sender, FocusEventArgs e)
    {
        if (AmountTextField.Text == "1")
        {
            AmountTextField.Text = "";
        }
    }
    private async void SelectUserFromContactsImgBtn_Clicked(object sender, EventArgs e)
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

            string namePrefix = PickedContact.DisplayName;
            PersonName.Text = namePrefix;
            PersonNumber.Text = PickedContact.Phones.FirstOrDefault()?.PhoneNumber;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Permission denied " + ex.Message);
        }
    }
    private void DeadlineSwitch_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {

        if (e.PropertyName == "IsToggled")
        {
            FlowHoldDeadline.Date = DateTime.Now;
        }
    }
}