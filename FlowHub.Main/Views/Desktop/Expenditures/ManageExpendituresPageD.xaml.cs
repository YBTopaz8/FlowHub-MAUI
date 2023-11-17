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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
       
        await viewModel.PageloadedAsync();

    }

    private async void ExportToPDFImageButton_Clicked(object sender, EventArgs e)
    {
        if (viewModel.ExpendituresList?.Count < 1)
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
}