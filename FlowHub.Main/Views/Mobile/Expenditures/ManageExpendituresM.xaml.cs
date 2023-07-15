using Microsoft.Maui.Controls.Platform;

namespace FlowHub.Main.Views.Mobile.Expenditures;

public partial class ManageExpendituresM : ContentPage
{
    private readonly ManageExpendituresVM viewModel;
    readonly Animation rotation;
    public ManageExpendituresM(ManageExpendituresVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;

        rotation = new Animation(v => SyncButton.Rotation = v,
            0, 360, Easing.Linear);

        Microsoft.Maui.Handlers
            .DatePickerHandler.Mapper
            .AppendToMapping("MyCustomDatePicker", (handler, view) =>
            {
                if (view is DatePicker)
                {
#if ANDROID
                    Android.Graphics.Drawables.GradientDrawable gd = new();
                    gd.SetColor(global::Android.Graphics.Color.Transparent);
                    handler.PlatformView.SetBackground(gd);
#endif
                }
            });

        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.IsBusy))
        {
            if (viewModel.IsBusy)
            {
                rotation.Commit(this, "RotateSyncButton", 16, 1000, Easing.Linear,
                    (value , b) => SyncButton.Rotation = 0,
                    () => true);
                ColView.IsVisible = false;
            }
            else
            {
                this.AbortAnimation("RotateSyncButton");
                ColView.IsVisible = true;
            }
        }
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

    private void FilterOption_Clicked(object sender, EventArgs e)
    {
        //var FadeOut = filterOptionsContainer.FadeTo(0, 350, easing: Easing.Linear);
        //var MoveUp = filterOptionsContainer.TranslateTo(0, -30,350, Easing.Linear);
        //var MoveColViewUp = ColView.TranslateTo(0, -30,350, Easing.Linear);
        //_ = await Task.WhenAll(FadeOut, MoveUp, MoveColViewUp);
        //filterExpander.IsExpanded = false;
        //filterOptionsContainer.TranslationY= 0;
        //ColView.TranslationY= 0;
        //await filterOptionsContainer.FadeTo(1, 0);
    }


    /*
     * This snippet can be used if i ever want to allow multi selection
    private void ColView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var previous = e.PreviousSelection;
        var current = e.CurrentSelection.ToList();
        
        Debug.WriteLine(current.GetType());
    }

    private void CheckBox_CheckChanged(object sender, EventArgs e)
    {
        if (selMode.IsChecked)
        {
            ColView.SelectionMode = SelectionMode.Multiple;
        }
        else
        {
            ColView.SelectedItems.Clear();
            ColView.SelectionMode = SelectionMode.None;
        }
    }
    */
}
