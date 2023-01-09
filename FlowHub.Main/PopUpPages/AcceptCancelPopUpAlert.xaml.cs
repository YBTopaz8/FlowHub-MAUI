using CommunityToolkit.Maui.Views;

namespace FlowHub.Main.PopUpPages;

public partial class AcceptCancelPopUpAlert : Popup
{
	public AcceptCancelPopUpAlert(string AlertText)
	{
		InitializeComponent();
		DisplayAlertText.Text = AlertText;

	}
    void OnYesButtonClicked(object sender, EventArgs e) => Close(true);

    void OnNoButtonClicked(object sender, EventArgs e) => Close(false);
}