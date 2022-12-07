using FlowHub.Main.ViewModels;

namespace FlowHub.Main.Views.Desktop;

public partial class LoginD : ContentPage
{
    private readonly LoginVM viewModel;
    public LoginD(LoginVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;

        viewModel.PageLoadedCommand.Execute(null);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        bool HasLoginRemembered = viewModel.HasLoginRemembered;
        bool isLoginFormVisible = viewModel.IsLoginFormVisible;

        ToggleFormAndValidation(HasLoginRemembered, isLoginFormVisible);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        LoginForm.IsVisible = true;
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

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Debug.WriteLine(CountryPicker.SelectedItem.ToString());
        string selectedCountry = CountryPicker.SelectedItem.ToString();
        viewModel.ErrorMessagePicker = false;
        viewModel.CurrencyFromCountryPickedCommand.Execute(selectedCountry);
    }

    private void ShowRegisterFormClick(object sender, TappedEventArgs e)
    {
        LoginForm.IsVisible = false;
        RegisterForm.IsVisible = true;

    }
    private void SwitchToLoginPageTapped(object sender, TappedEventArgs e)
    {
        LoginForm.IsVisible = true;
        RegisterForm.IsVisible = false;
    }
}