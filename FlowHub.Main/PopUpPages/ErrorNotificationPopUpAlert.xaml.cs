using CommunityToolkit.Maui.Views;

namespace FlowHub.Main.PopUpPages;

public partial class ErrorNotificationPopUpAlert : Popup
{
	public ErrorNotificationPopUpAlert(string ErrorText)
	{
		InitializeComponent();
		DisplayErrorText.Text = ErrorText;
	}

	void OnOKButtonClicked(object sender, EventArgs e) => Close();
}