using Maui.DataGrid;

namespace FlowHub.Main.Views.Desktop.Expenditures;

public partial class ManageExpendituresPageD : ContentPage
{
    readonly ManageExpendituresVM viewModel;


    public ManageExpendituresPageD(ManageExpendituresVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.Pageloaded();

    }

    private async void ExportToPDFImageButton_Clicked(object sender, EventArgs e)
    {
        if (viewModel.ExpendituresCollection?.Count < 1)
        {
            await Shell.Current.ShowPopupAsync(new ErrorPopUpAlert("Cannot Save an Empty List to PDF"));
        }
        else
        {
            //PrintProgressBarIndic.IsVisible = true;
            //PrintProgressBarIndic.Progress = 0;
            //await PrintProgressBarIndic.ProgressTo(1, 1000, easing: Easing.Linear);

            //await viewModel.PrintExpendituresBtn();
            //PrintProgressBarIndic.IsVisible = false;
        }
    }

    private CancellationTokenSource _cts = new();
    private async void ExpenditureSearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        /*
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        var cancellationToken = _cts.Token;
        try
        {
            await Task.Delay(350, cancellationToken);
            if (!cancellationToken.IsCancellationRequested)
            {
                SearchBar searchBar = sender as SearchBar;
                if (searchBar.Text.Length >= 1)
                {
                    viewModel.SearchExpendituresCommand.Execute(searchBar.Text);
                }
                else
                {
                    viewModel.ApplyChanges();
                }

            }
        }
        catch (TaskCanceledException ex)
        {

            Debug.WriteLine(ex.Message);
        }*/
    }
}