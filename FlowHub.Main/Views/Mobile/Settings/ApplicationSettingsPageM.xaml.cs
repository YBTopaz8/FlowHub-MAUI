using System.Diagnostics;

namespace FlowHub.Main.Views.Mobile.Settings;

public partial class ApplicationSettingsPageM : ContentPage
{
	public ApplicationSettingsPageM()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("===========================Appeared to app settings");
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        Debug.WriteLine("============================Navigated to app settings");
    }
}