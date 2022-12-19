using FlowHub.Main.ViewModels;

namespace FlowHub.Main.Views.Mobile;

public partial class LoginM : ContentPage
{
    private readonly LoginVM viewModel;
    public LoginM(LoginVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        viewModel.PageLoadedCommand.Execute(null);
        base.OnAppearing();
        bool HasLoginRemembered = viewModel.HasLoginRemembered;
        bool isLoginFormVisible = viewModel.IsLoginFormVisible;

        ToggleFormAndValidation(HasLoginRemembered, isLoginFormVisible);
    }
    private void ToggleFormAndValidation(bool HasLoginRemembered, bool isLoginVisible)
    {
        if (HasLoginRemembered && !isLoginVisible)
        {
            QuickLogin.IsVisible = true;
            LoginForm.IsVisible = false;
            RegisterForm.IsVisible = false;
        }
        else
        if (!HasLoginRemembered)
        {
            QuickLogin.IsVisible = false;
            RegisterForm.IsVisible = false;
            LoginForm.IsVisible = true;
        }

    }
    private void Button_Clicked(object sender, EventArgs e)
    {
        LoginForm.IsVisible = true;
    }


    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Debug.WriteLine(CountryPicker.SelectedItem.ToString());
        string selectedCountry = CountryPicker.SelectedItem.ToString();
        viewModel.CurrencyFromCountryPickedCommand.Execute(selectedCountry);
    }

    private void SwitchToRegisterPageTapped(object sender, TappedEventArgs e)
    {
        LoginForm.IsVisible = false;
        RegisterForm.IsVisible = true;
    }
    private void SwitchToLoginPageTapped(object sender, TappedEventArgs e)
    {
        LoginForm.IsVisible = true;
        RegisterForm.IsVisible = false;
        QuickLogin.IsVisible = false;
    }

    private void LoginButton_Clicked(object sender, EventArgs e)
    {
        //LoginForm.IsVisible = viewModel.IsLoginFormVisible;
        //RegisterForm.IsVisible = viewModel.IsRegisterFormVisible;
        //QuickLogin.IsVisible = viewModel.IsQuickLoginVisible;
    }
}