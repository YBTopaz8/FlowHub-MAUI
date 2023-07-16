namespace FlowHub.Main.Views.Mobile.Debts;

public partial class ManageDebtsPageM : ContentPage
{
	public ManageDebtsPageM()
	{
		InitializeComponent();
	}
    private async void OnFabClicked(object sender, EventArgs e)
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
			string namePrefix = PickedContact.NamePrefix is null ? PickedContact.GivenName
				+ (PickedContact.MiddleName ?? "")
				+ " " + PickedContact.FamilyName : PickedContact.FamilyName;
			contactName.Text = namePrefix;
		}
		catch (Exception ex)
		{
			Debug.WriteLine("Permission denied "+ ex.Message);
		}
    }
}