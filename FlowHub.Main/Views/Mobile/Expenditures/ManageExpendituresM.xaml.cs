using FlowHub.Main.ViewModels.Expenditures;

namespace FlowHub.Main.Views.Mobile.Expenditures;

public partial class ManageExpendituresM : ContentPage
{
    private ManageExpendituresVM viewModel;
    public ManageExpendituresM(ManageExpendituresVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageloadedCommand.Execute(null);
    }

    private async void ExportToPDFImageButton_Clicked(object sender, EventArgs e)
    {
        PrintProgressBarIndic.IsVisible = true;
        PrintProgressBarIndic.Progress = 0;
        await PrintProgressBarIndic.ProgressTo(1, 1500, easing: Easing.Linear);

        //PrintingActivIndic.IsVisible = true;
        //PrintingActivIndic.IsRunning = true;
        await viewModel.PrintExpendituresBtnCommand.ExecuteAsync(null);
        // bool keepRunning = viewModel.Activ;  //viewModel.PrintExpendituresBtnCommand.ExecuteAsync(null);
        PrintProgressBarIndic.IsVisible = false;
    }
}
