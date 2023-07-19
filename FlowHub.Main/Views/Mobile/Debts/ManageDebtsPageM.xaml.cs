namespace FlowHub.Main.Views.Mobile.Debts;

public partial class ManageDebtsPageM : ContentPage
{
    readonly ManageDebtsVM viewModel;

    public ManageDebtsPageM(ManageDebtsVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoaded();
    }
    private async void OnFabClicked(object sender, EventArgs e)
    {
    }

    private async void OnContactClicked(object sender, EventArgs e)
    {
        /*
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
			contactName.Text = namePrefix;
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Permission denied "+ ex.Message);
		}*/
    }
}