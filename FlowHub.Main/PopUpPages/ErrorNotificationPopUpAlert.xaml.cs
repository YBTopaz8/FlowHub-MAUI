namespace FlowHub.Main.PopUpPages;

public partial class ErrorNotificationPopUpAlert : Popup
{
	public ErrorNotificationPopUpAlert(string ErrorText="Nothing")
	{
		InitializeComponent();
		DisplayErrorText.Text = ErrorText;
	}

	void OnOKButtonClicked(object sender, EventArgs e) => Close();
}