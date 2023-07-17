namespace FlowHub.Main.Views.Mobile.Expenditures.PlannedExpenditures.MonthlyPlannedExp;

public partial class ManageMonthlyPlannedExpendituresPageM : ContentPage
{
	private ManageMonthlyMonthlyPlannedExpendituresVM viewModel;
	public ManageMonthlyPlannedExpendituresPageM(ManageMonthlyMonthlyPlannedExpendituresVM vm)
	{
		InitializeComponent();
		viewModel= vm;
		BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.PageLoaded();
    }

    private void MultiSelectToggle_CheckChanged(object sender, EventArgs e)
    {
        if (MultiSelectToggle.IsChecked)
        {
            ColView.SelectionMode = SelectionMode.Multiple;
        }
        else
        {
            ColView.SelectionMode = SelectionMode.None;
            if (ListOfExps is not null)
            {
                ListOfExps.Clear();
                ListOfTitles.Clear();
            }
        }
    }
    List<List<ExpendituresModel>> ListOfExps { get;set; }
    List<string> ListOfTitles { get;set; }
    private void ColView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ListOfExps = new();
        ListOfTitles = new();
        var item = e.CurrentSelection;
        foreach (PlannedExpendituresModel value in item)
        {
            ListOfExps.Add(value.Expenditures);
            ListOfTitles.Add(value.Title);
        }
    }

    private async void ExportToPDFImageButton_Clicked(object sender, EventArgs e)
    {
        if (ListOfExps is not null && ListOfExps.Count != 0)
        {
            await viewModel.PrintPDFandShare(ListOfExps, ListOfTitles);
        }
        else
        {
            await Shell.Current.DisplayAlert("Error !", "Please Select Items first!", "Ok");
        }
    }
}