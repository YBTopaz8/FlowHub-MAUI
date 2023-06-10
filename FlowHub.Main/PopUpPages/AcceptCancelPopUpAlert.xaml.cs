using CommunityToolkit.Maui.Views;

namespace FlowHub.Main.PopUpPages;

public partial class AcceptCancelPopUpAlert : Popup
{
	public AcceptCancelPopUpAlert(string AlertText, string AlertTitle = "Confirm Action")
	{
		InitializeComponent();
		DisplayAlertText.Text = AlertText;
		DisplayAlertTitle.Text = AlertTitle;
	}
    void OnYesButtonClicked(object sender, EventArgs e) => Close(true);

    void OnNoButtonClicked(object sender, EventArgs e) => Close(false);
}