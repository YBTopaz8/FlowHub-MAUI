using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace FlowHub.Main.PopUpPages;

public partial class FilterOptionsPopUp : Popup
{
    List<string> FilterResult { get; set; }
    List<string> InitialResultForCancel { get; set; }
    string selectedMonth { get; set; } = "null";

    readonly List<string> ListOfMonths = new()
        { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

    readonly List<string> ListOfYears = new()
        { "2019", "2020", "2021", "2022", "2023" , "2024", "2025" };

    public FilterOptionsPopUp(string InitialFilterOption, int specificMonth=0, int specificYear=0)
    {
        InitializeComponent();

        FilterResult = new List<string>
        {
            InitialFilterOption
        };
        InitialResultForCancel = new List<string>()
        {
             InitialFilterOption
        };

        MonthPicker.ItemsSource = ListOfMonths;
        YearPicker.ItemsSource = ListOfYears;


        if (specificMonth == 0 && specificYear == 0)
        {
            int monthIndex = DateTime.UtcNow.Month - 1;
            MonthPicker.SelectedIndex = monthIndex;
            selectedMonth = ReturnFirstThreeLetters(ListOfMonths[monthIndex]);
            YearPicker.SelectedItem = DateTime.UtcNow.Year.ToString();
        }
        else
        {
            int monthIndex = specificMonth - 1;
            MonthPicker.SelectedIndex = monthIndex;
            selectedMonth = ReturnFirstThreeLetters( ListOfMonths[monthIndex]);
            YearPicker.SelectedItem = specificYear.ToString();
        }

        FilterOptionsRadioGroup.SelectedItem = InitialFilterOption;
    }

    private void FilterOptionsRadioGroup_SelectedItemChanged(object sender, EventArgs e)
    {

        var radioGroup = (UraniumUI.Material.Controls.RadioButtonGroupView)sender;
        if (radioGroup.SelectedItem.ToString() == "Filter_Spec_Month")
        {
            FilterResult = new List<string>
            {
                radioGroup.SelectedItem.ToString(),
                (MonthPicker.SelectedIndex + 1).ToString(),
                YearPicker.SelectedItem.ToString(),
                selectedMonth
            };
        }
        else
        {
            FilterResult = new List<string>
            {
                radioGroup.SelectedItem.ToString()
            };
        }

        
    }

    private void MonthPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (Filter_Spec_Month.IsChecked)
        {
            FilterResult[1] = (MonthPicker.SelectedIndex + 1).ToString();
            FilterResult[3] = ReturnFirstThreeLetters(MonthPicker.SelectedItem.ToString());
        }
    }

    private void YearPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (Filter_Spec_Month.IsChecked)
        {
            FilterResult[2] = YearPicker.SelectedItem.ToString();
        }
    }

    void OnOKButtonClicked(object sender, EventArgs e) => Close(FilterResult );
    void OnCancelButtonClicked(object sender, EventArgs e) => Close(InitialResultForCancel);

    private string ReturnFirstThreeLetters(string SelectedMonthParam)
    {
        return SelectedMonthParam[..3];
    }

}