using CommunityToolkit.Maui.Views;
using FlowHub.Main.PopUpPages;
using FlowHub.Main.ViewModels.Expenditures;
using System.Diagnostics;

namespace FlowHub.Main.Views.Mobile.Expenditures;

public partial class ManageExpendituresM : ContentPage
{
    private readonly ManageExpendituresVM viewModel;
    public ManageExpendituresM(ManageExpendituresVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageloadedAsyncCommand.Execute(null);
        
    }

    private async void ExportToPDFImageButton_Clicked(object sender, EventArgs e)
    {
        if (viewModel.ExpendituresList?.Count < 1)
        {
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("Cannot Save an Empty List to PDF"));
        }
        else
        {
            PrintProgressBarIndic.IsVisible = true;
            PrintProgressBarIndic.Progress = 0;
            await PrintProgressBarIndic.ProgressTo(1, 1000, easing: Easing.Linear);

            await viewModel.PrintExpendituresBtnCommand.ExecuteAsync(null);
            PrintProgressBarIndic.IsVisible = false;

        }
    }

}
