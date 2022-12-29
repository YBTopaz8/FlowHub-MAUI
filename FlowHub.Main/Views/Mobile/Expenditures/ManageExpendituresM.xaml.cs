using CommunityToolkit.Maui.Views;
using FlowHub.Main.PopUpPages;
using FlowHub.Main.ViewModels.Expenditures;
using Microsoft.Maui.Controls.Platform;
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

        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("MyCustomDatePicker", (handler, view) =>
        {
            if (view is DatePicker)
            {
#if ANDROID
                Android.Graphics.Drawables.GradientDrawable gd = new Android.Graphics.Drawables.GradientDrawable();
                gd.SetColor(global::Android.Graphics.Color.Transparent);
                handler.PlatformView.SetBackground(gd);
#endif
            }
        });

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

    private async void FilterOption_Clicked(object sender, EventArgs e)
    {
        var FadeOut = filterOptionsContainer.FadeTo(0, 350, easing: Easing.Linear);
        var MoveUp = filterOptionsContainer.TranslateTo(0, -30,350, Easing.Linear);
        var MoveColViewUp = ColView.TranslateTo(0, -30,350, Easing.Linear);
        _ = await Task.WhenAll(FadeOut, MoveUp, MoveColViewUp);
        filterExpander.IsExpanded = false;
        filterOptionsContainer.TranslationY= 0;
        ColView.TranslationY= 0;
        await filterOptionsContainer.FadeTo(1, 0);                
    }


}
