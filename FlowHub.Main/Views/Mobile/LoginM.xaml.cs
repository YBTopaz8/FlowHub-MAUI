using FlowHub.Main.ViewModels;
using Microsoft.Maui.Controls.Platform;
using System.Diagnostics;

namespace FlowHub.Main.Views.Mobile;

public partial class LoginM : ContentPage
{
    private readonly LoginVM viewModel;
    public LoginM(LoginVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;

        //to remove picker's underline

        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("MyCustomization", (handler, view) =>
        {
            if (view is Picker)
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
        viewModel.PageLoadedCommand.Execute(null);
        base.OnAppearing();
        bool HasLoginRemembered = viewModel.HasLoginRemembered;
        bool isLoginFormVisible = viewModel.IsLoginFormVisible;

        ToggleFormAndValidation(HasLoginRemembered, isLoginFormVisible);

    }

    private async void SignUpUnFocused_Tapped(object sender, TappedEventArgs e)
    {
        await ShowRegisterForm();
    }

    private async Task ShowRegisterForm()
    {
        LoginForm.IsVisible = false;
        SignUpUnFocused.IsVisible = false;

        RegisterForm.IsVisible = true;
        LoginUnFocused.IsVisible = true;
        SignUpFocused.IsVisible = true;
        LoginFocused.IsVisible = false;

        await Task.WhenAll(BorderFadeIn(LoginUnFocused), BorderFadeIn(SignUpFocused),
            VSLayoutFadeIn(RegisterForm),
            BorderFadeOut(SignUpUnFocused),
            BorderFadeOut(LoginFocused),
            VSLayoutFadeOut(LoginForm));
    }

    private async void LoginUnFocused_Tapped(object sender, TappedEventArgs e)
    {
        await ShowLoginForm();
    }

    private async Task ShowLoginForm()
    {
        LoginForm.IsVisible = true;
        SignUpUnFocused.IsVisible = true;

        RegisterForm.IsVisible = false;
        LoginUnFocused.IsVisible = false;
        SignUpFocused.IsVisible = false;
        LoginFocused.IsVisible = true;

        _ = await Task.WhenAll(BorderFadeOut(LoginUnFocused), BorderFadeOut(SignUpFocused),
            VSLayoutFadeOut(RegisterForm),
            BorderFadeIn(LoginFocused),
            BorderFadeIn(SignUpUnFocused),
            VSLayoutFadeIn(LoginForm));
    }

    private void ToggleFormAndValidation(bool HasLoginRemembered, bool isLoginVisible)
    {
        if (HasLoginRemembered && !isLoginVisible)
        {
            QuickLogin.IsVisible = true;
            LoginForm.IsVisible = false;
            RegisterForm.IsVisible = false;
            LoginSignUpTab.IsVisible = false;
        }
        else
        if (!HasLoginRemembered)
        {
            QuickLogin.IsVisible = false;
            RegisterForm.IsVisible = false;
            LoginForm.IsVisible = true;
            LoginSignUpTab.IsVisible = true;
        }

    }
   
    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selectedCountry = CountryPicker.SelectedItem.ToString();
        viewModel.CurrencyFromCountryPickedCommand.Execute(selectedCountry);
    }

    private void SwitchToLoginPageTapped(object sender, TappedEventArgs e)
    {
        LoginSignUpTab.IsVisible = true;
        LoginForm.IsVisible = true;
        RegisterForm.IsVisible = false;
        QuickLogin.IsVisible = false;
    }

    private void QuickLoginBtn_Clicked(object sender, EventArgs e)
    {
        QuickLoginBtn.IsEnabled = false;
        viewModel.QuickLoginCommand.Execute(null);
    }

    


    uint animationSpeed = 300;
    Easing animationIn = Easing.CubicIn;
    Easing animationOut = Easing.CubicOut;
    Task<bool> VSLayoutFadeOut(VerticalStackLayout Form)
    {
        return Form.FadeTo(0, animationSpeed, animationOut);
    }
    Task<bool> VSLayoutFadeIn(VerticalStackLayout Form)
    {
        return Form.FadeTo(1, animationSpeed, animationIn);
    }
    Task<bool> BorderFadeOut(Border border)
    {
        return border.FadeTo(0, animationSpeed, animationOut);
    }
    Task<bool> BorderFadeIn(Border border)
    {
        return border.FadeTo(1, animationSpeed, animationIn);
    }

}