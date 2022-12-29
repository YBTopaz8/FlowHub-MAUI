using CommunityToolkit.Maui.Views;
using FlowHub.Main.PopUpPages;
using FlowHub.Main.ViewModels.Incomes;
using System.Diagnostics;

namespace FlowHub.Main.Views.Mobile.Incomes;

public partial class ManageIncomesM : ContentPage
{
    private readonly ManageIncomesVM viewModel;
    List<string> FilterResult { get; set; } 
    public ManageIncomesM(ManageIncomesVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;

        FilterResult= new List<string>() { "12", "2022" };
        List<string> ListOfMonths = new()
        { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        MonthPicker.ItemsSource = ListOfMonths;

        List<string> ListOfYears = new()
        { "2019", "2020", "2021", "2022", "2023" , "2024", "2025", "2026", "2027", "2028", "2029", "2030", "2031" };
        YearPicker.ItemsSource = ListOfYears;

        int monthIndex = DateTime.UtcNow.Month;

        MonthPicker.SelectedIndex = monthIndex - 1;
        YearPicker.SelectedItem = DateTime.UtcNow.Year.ToString();
        if (FilterResult[1] is null)
        {
            if (FilterResult[0] == "Filter_Today")
            {
                Filter_Today.IsChecked = true;
            }
            else if (FilterResult[0] == "Filter_Curr_Month")
            {
                Filter_Curr_Month.IsChecked = true;
            }
            else if (FilterResult[0] == "Filter_All")
            {
                Filter_All.IsChecked = true;
            }
        }
        else
        {
            MonthPicker.SelectedIndex = int.Parse(FilterResult[0]) - 1;
            YearPicker.SelectedItem = FilterResult[1];
            Filter_Spec_Month.IsChecked = true;
        }
    }

    protected override void OnAppearing()
    {
        
        base.OnAppearing();
        viewModel.PageLoadedCommand.Execute(null);
        
    }
    private async void ExportToPDFImageButton_Clicked(object sender, EventArgs e)
    {
        if (viewModel.IncomesList?.Count < 1)
        {
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("Cannot Save an Empty List to PDF"));
        }
        else
        {
            PrintProgressBarIndic.IsVisible = true;
            PrintProgressBarIndic.Progress = 0;
            await PrintProgressBarIndic.ProgressTo(1, 1000, easing: Easing.Linear);

            await viewModel.PrintIncomesBtnCommand.ExecuteAsync(null);
            PrintProgressBarIndic.IsVisible = false;
        }
    }
    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
      await  Shell.Current.ShowPopupAsync(new InputPopUpPage(isNumericInput: true, optionalTitleText:"Enter New Pocket Money"));
    }
    
    private void View_other_filters_RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {

        var filterBtn = (RadioButton)sender;
        if (filterBtn.IsChecked)
        {
            Debug.WriteLine("View Other Filters ");
            FilterResult = filterBtn.Value.ToString().Split(' ').ToList();
            Debug.WriteLine(FilterResult.Count);
        }
    }

    private void View_Spec_Month_RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
       
        var s = (RadioButton)sender;
        if (s.IsChecked)
        {
            Debug.WriteLine("View Specific Month");
            FilterResult = new List<string>
            {
                (MonthPicker.SelectedIndex + 1).ToString(),
                YearPicker.SelectedItem.ToString()
            };
            Debug.WriteLine(FilterResult.Count);
        }
    }

    private void ValidateSpecificMonthYear_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine("Validate Specific Month");
        Debug.WriteLine(FilterResult[0]);
        Debug.WriteLine(FilterResult[1]);
        
    }

    private void okayFilter_Clicked(object sender, EventArgs e)
    {
        Debug.WriteLine(FilterResult.Count);
        if (FilterResult.Count == 1)
        {
            Debug.WriteLine(FilterResult[0]);
        }
        else
        {
            Debug.WriteLine(FilterResult[0]);
            Debug.WriteLine(FilterResult[1]);
        }
    }

    private void MonthPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (Filter_Spec_Month.IsChecked)
        {
            FilterResult[0] = (MonthPicker.SelectedIndex + 1).ToString();
        }
    }

    private void YearPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (Filter_Spec_Month.IsChecked)
        {
            FilterResult[1] = YearPicker.SelectedItem.ToString();
        }
    }

    private void RadioButtonGroupView_SelectedItemChanged(object sender, EventArgs e)
    {
        var s = (UraniumUI.Material.Controls.RadioButtonGroupView)sender;
        Debug.WriteLine(s.SelectedItem);
    }
}
