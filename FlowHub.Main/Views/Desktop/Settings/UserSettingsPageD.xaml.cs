namespace FlowHub.Main.Views.Desktop.Settings;

public partial class UserSettingsPageD : ContentPage
{   
    UserSettingsVM viewModel;
    public UserSettingsPageD(UserSettingsVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        
        viewModel.SetThemeConfig();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoadedCommand.Execute(null);
        viewModel.GetCountryNamesList();
        CountryPicker.SelectedItem = viewModel.ActiveUser.UserCountry;
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        this.BindingContext = viewModel;
    }
    private void CountryPicker_SelectedValueChanged(object sender, object e)
    {
        var pickedCountry = CountryPicker.SelectedItem;
        if (pickedCountry is not null)
        {
            viewModel.CurrencyFromCountryPickedCommand.Execute(pickedCountry.ToString());
        }
    }

    private void EditUserDetailsBtn_Clicked(object sender, EventArgs e)
    {
        if (UserDetailsView.IsVisible)
        {
            //viewModel.IsNotInEditingMode = false;
        }
    }
}