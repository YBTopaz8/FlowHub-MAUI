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
       // viewModel.PropertyChanged += ViewModel_PropertyChanged;

        SizeChanged += ManageExpendituresD_SizeChanged;
    }

    private void ManageExpendituresD_SizeChanged(object sender, EventArgs e)
    {
        //if (page.Width > 1280)
        //{
        //    dockLeft.WidthRequest = 400;
        //    DGScrollView.Margin = new Thickness(0, 0, 300, 0);
        //}
        //else
        //{
        //    dockLeft.WidthRequest = 210;
        //    DGScrollView.Margin = new Thickness(0, 0, 130, 0);

        //}
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageloadedAsyncCommand.Execute(null);
        dateSort.CommandParameter = 0;
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
                //  ColView.IsVisible = false;
            }
            else
            {
                this.AbortAnimation("RotateSyncButton");
               // ColView.IsVisible = true;
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

    
}