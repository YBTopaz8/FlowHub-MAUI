using Microsoft.Maui.Controls.Platform;
using UraniumUI.Material.Controls;

namespace FlowHub.Main.Views.Mobile;

public partial class LoginM : ContentPage
{
    private readonly LoginVM viewModel;
    public LoginM(LoginVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;

        //to remove picker's underline

        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("MyCustomization", (handler, view) =>
        {
            if (view is Picker)
            {
#if ANDROID
                Android.Graphics.Drawables.GradientDrawable gd = new();
                gd.SetColor(Android.Graphics.Color.Transparent);

                handler.PlatformView.SetBackground(gd);

#endif
            }
        });
    }
    protected override async void OnAppearing()
    {
        await viewModel.PageLoaded();
        base.OnAppearing();
        bool HasLoginRemembered = viewModel.HasLoginRemembered;
        bool isLoginFormVisible = viewModel.IsLoginFormVisible;

        ToggleFormAndValidation(HasLoginRemembered, isLoginFormVisible);
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

    private void SwitchToLoginPageTapped(object sender, TappedEventArgs e)
    {
        LoginSignUpTab.IsVisible = true;
        LoginForm.IsVisible = true;
        RegisterForm.IsVisible = false;
        QuickLogin.IsVisible = false;
    }

    private async void QuickLoginBtn_Clicked(object sender, EventArgs e)
    {
        QuickLoginBtn.IsEnabled = false;
        await viewModel.QuickLogin();
    }

    private async void LoginUnFocused_Tapped(object sender, TappedEventArgs e)
    {
        TextField s = new();
        Entry ss = new();
        Image sss = new();

        await ShowLoginForm();
    }

    private async void SignUpUnFocused_Tapped(object sender, TappedEventArgs e)
    {
        await ShowRegisterForm();
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

    private void LoginOnlineBtn_Clicked(object sender, EventArgs e)
    {
        viewModel.IsLoginOnlineButtonClicked = true;
    }
}