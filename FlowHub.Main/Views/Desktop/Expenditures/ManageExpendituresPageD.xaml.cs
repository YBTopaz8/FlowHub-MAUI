using CommunityToolkit.Maui.Views;
using FlowHub.Main.PopUpPages;
using FlowHub.Main.ViewModels.Expenditures;
using Maui.DataGrid;

namespace FlowHub.Main.Views.Desktop.Expenditures;

public partial class ManageExpendituresPageD : ContentPage
{
	readonly ManageExpendituresVM viewModel;
    readonly Animation rotation;
    public ManageExpendituresPageD(ManageExpendituresVM vm)
	{
		InitializeComponent();
		viewModel = vm;
		this.BindingContext = vm;
        rotation = new Animation(v => SyncButton.Rotation = v,
            0, 360, Easing.Linear);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.PageloadedAsync();


        //ExpDG.SortedColumnIndex = 0;
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.IsBusy))
        {
            if (viewModel.IsBusy)
            {
                rotation.Commit(this, "RotateSyncButton", 16, 1000, Easing.Linear,
                    (__, _) => SyncButton.Rotation = 0,
                    () => true);
            }
            else
            {
                this.AbortAnimation("RotateSyncButton");
            }
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

            await viewModel.PrintExpendituresBtn();
            PrintProgressBarIndic.IsVisible = false;
        }
    }
}