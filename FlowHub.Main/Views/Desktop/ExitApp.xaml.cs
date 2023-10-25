namespace FlowHub.Main.Views.Desktop;

public partial class ExitApp : ContentPage
{
	public ExitApp()
	{
		InitializeComponent();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();

        OnExitAppearing();
    }

    async void OnExitAppearing()
    {
        bool result = await DisplayAlert("Confirm Exit", "Do You Want To Exit Application?", "Yes", "No");
        //bool result = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("ss"));
        if (result)
        {
            Environment.Exit(0);
        }
        else
        {
            await Shell.Current.GoToAsync($"//{nameof(HomePageD)}", true);
            Debug.WriteLine("Cancelled Exit");
        }
    }
}