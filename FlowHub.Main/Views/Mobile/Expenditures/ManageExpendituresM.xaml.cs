using Microsoft.Maui.Controls.Platform;

namespace FlowHub.Main.Views.Mobile.Expenditures;

public partial class ManageExpendituresM : ContentPage
{
    private readonly ManageExpendituresVM viewModel;
    
    public ManageExpendituresM(ManageExpendituresVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;

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
            PrintProgressBarIndic.IsVisible = true;
            PrintProgressBarIndic.Progress = 0;
            await PrintProgressBarIndic.ProgressTo(1, 1000, easing: Easing.Linear);

            await viewModel.PrintExpendituresBtn();
            PrintProgressBarIndic.IsVisible = false;
        }
    }
}
