namespace FlowHub.Main.Views.Mobile.Debts;

public partial class UpSertDebtPageM : ContentPage
{
    private readonly UpSertDebtVM viewModel;

    public UpSertDebtPageM(UpSertDebtVM vm)
	{
		InitializeComponent();
        BindingContext = vm;
        viewModel = vm;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
    private async void ImageButton_Clicked(object sender, EventArgs e)
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

    private void TextField_Focused(object sender, FocusEventArgs e)
    {
        if (AmountTextField.Text == "0")
        {
            AmountTextField.Text = "";
        }
    }
}