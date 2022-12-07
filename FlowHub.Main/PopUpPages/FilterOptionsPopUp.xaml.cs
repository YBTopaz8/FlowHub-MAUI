using CommunityToolkit.Maui.Views;
namespace FlowHub.Main.PopUpPages;

public partial class FilterOptionsPopUp : Popup
{
	public FilterOptionsPopUp()
	{
		InitializeComponent();
	}


    void Filter_Today_Tapped(object sender, TappedEventArgs e) => Close("Filter_Today");

    void Filter_Curr_Month_Tapped(object sender, TappedEventArgs e) => Close("Filter_CurrMonth");

    private void Filter_All_Tapped(object sender, TappedEventArgs e) => Close("Filter_All");
    void OnCancelButtonClicked(object sender, EventArgs e) => Close("Cancelled");
}