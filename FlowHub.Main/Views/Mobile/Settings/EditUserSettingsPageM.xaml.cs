namespace FlowHub.Main.Views.Mobile.Settings;

public partial class EditUserSettingsPageM : ContentPage
{
    private UserSettingsVM viewmodel;
	public EditUserSettingsPageM(UserSettingsVM vm)
	{
		InitializeComponent();
        viewmodel = vm;
        BindingContext = vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        //viewmodel.PageLoaded();
        viewmodel.PageLoadedCommand.Execute(null);
        viewmodel.GetCountryNamesList();
        //TODO : set this in binding instead of code behind
        CountryPicker.SelectedItem = viewmodel.ActiveUser.UserCountry;
    }

    private void CountryPicker_SelectedValueChanged(object sender, object e)
    {
        var pickedCountry = CountryPicker.SelectedItem;
        if (pickedCountry is not null)
        {
            viewmodel.CurrencyFromCountryPickedCommand.Execute(pickedCountry.ToString());
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Debug.WriteLine("Tapped");
    }
}