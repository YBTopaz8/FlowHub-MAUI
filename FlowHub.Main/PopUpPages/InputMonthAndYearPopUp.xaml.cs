using CommunityToolkit.Maui.Views;

namespace FlowHub.Main.PopUpPages;

public partial class InputMonthAndYearPopUp : Popup
{
	public InputMonthAndYearPopUp()
	{
		InitializeComponent();

        List<string> ListOfMonths = new() 
        { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        MonthPicker.ItemsSource = ListOfMonths;

        List<string> ListOfYears = new()
        { "2019", "2020", "2021", "2022", "2023" , "2024", "2025" };
        YearPicker.ItemsSource = ListOfYears;

        int monthIndex = DateTime.UtcNow.Month;

        MonthPicker.SelectedIndex =  monthIndex - 1; 
        YearPicker.SelectedItem = DateTime.UtcNow.Year.ToString();
	}

    void OnYesButtonClicked(object sender, EventArgs e)
    {
        string MonthValue =  MonthPicker.SelectedItem.ToString();
        string YearValue = YearPicker.SelectedItem.ToString();
        
        string MonthYear = $"{MonthValue}, {YearValue}";
        Close(MonthYear);
    }

    void OnNoButtonClicked(object sender, EventArgs e) => Close("Cancel");
}