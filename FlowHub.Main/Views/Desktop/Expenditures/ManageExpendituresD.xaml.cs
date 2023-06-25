using CommunityToolkit.Maui.Views;
using FlowHub.Main.PopUpPages;
using FlowHub.Main.ViewModels.Expenditures;
using System.Diagnostics;

namespace FlowHub.Main.Views.Desktop.Expenditures;

public partial class ManageExpendituresD : ContentPage
{
    private ManageExpendituresVM viewModel;
    readonly Animation rotation;
    public ManageExpendituresD(ManageExpendituresVM vm)
	{
		InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;

        rotation = new Animation(v => SyncButton.Rotation = v,
            0, 360, Easing.Linear);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        //viewModel.PageloadedAsyncCommand.Execute(null);
        //dateSort.CommandParameter = 0;
    }
    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.IsBusy))
        {
            if (viewModel.IsBusy)
            {
                rotation.Commit(this, "RotateSyncButton", 16, 1000, Easing.Linear,
                    (value, b) => SyncButton.Rotation = 0,
                    () => true);
            }
            else
            {
                this.AbortAnimation("RotateSyncButton");
            }
        }
    }

    private void DateSpent_Tapped(object sender, TappedEventArgs e)
    {
        if (upBtn.IsVisible)
        {//show sorted by DESC
         // viewModel.SortingCommand.Execute(1);

             viewModel.SortingCommand.Execute(1);

            upBtn.IsVisible = false;
            downBtn.IsVisible = true;
        }
        else
        {
             viewModel.SortingCommand.Execute(0);

            upBtn.IsVisible = true;
            downBtn.IsVisible = false;
        }
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        if (upBtn.IsVisible)
        {//show sorted by DESC
            viewModel.SortingCommand.Execute(1);

            upBtn.IsVisible = false;
            downBtn.IsVisible = true;
        }
        else
        {
            viewModel.SortingCommand.Execute(0);

            upBtn.IsVisible = true;
            downBtn.IsVisible = false;
        }
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