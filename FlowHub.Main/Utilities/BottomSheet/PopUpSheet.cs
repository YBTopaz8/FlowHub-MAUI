using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowHub.Main.Utilities.BottomSheet;

public class PopUpSheet : ContentPage
{
    public bool IsFadeBackground = true;
    private ImageButton BackgroundBack;
    private static bool isBusy;

    public PopUpSheet()
    {
        this.BackgroundColor = Color.FromArgb("#01000000");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (IsFadeBackground)
            BackgroundBack.FadeTo(.4, 500);
    }

    protected override void OnParentChanged()
    {
        base.OnParentChanged();
        var cache = this.Content;
        BackgroundBack = new ImageButton()
        {
            BackgroundColor = IsFadeBackground ? Colors.Black : Colors.Transparent,
            Opacity = IsFadeBackground ? 0 : 1
        };
        var bugdroid = new TapGestureRecognizer();
        cache.GestureRecognizers.Add(bugdroid);

        this.Content = new Grid() { BackgroundBack, cache };
        BackgroundBack.Clicked -= OnCloseBackgroundClicked;
        BackgroundBack.Clicked += OnCloseBackgroundClicked;
    }

    async void OnCloseBackgroundClicked(object sender, EventArgs args)
    {
        if (IsCloseOnBackgroundClick)
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Close();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    public TaskCompletionSource<object> CallBackResult = new();

    public bool IsCloseOnBackgroundClick { get; set; } = true;

    protected override bool OnBackButtonPressed()
    {
        if (!IsCloseOnBackgroundClick)
            return true;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Close();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        return true;
    }

    public async virtual Task BeforeOpen()
    {
        //max delay 500
    }

    public async virtual Task AfterOpen()
    {
    }

    public async virtual Task BeforeClose()
    {
        //max delay 400
    }

    public async virtual Task AfterClose()
    {
    }

    public static async Task<T> Open<T>(PopUpSheet page) where T : new()
    {
        if (isBusy)
#pragma warning disable IDE0034 // Simplify 'default' expression
            return default(T);
#pragma warning restore IDE0034 // Simplify 'default' expression
        isBusy = true;
#pragma warning disable CS0168 // Variable is declared but never used
        try
        {
            if (Application.Current?.MainPage != null)
            {
                await page.BeforeOpen();
                await Application.Current.MainPage.Navigation.PushModalAsync(page, false);
                await page.AfterOpen();
            }

            return (T)await page.CallBackResult.Task;
        }
        catch (Exception ex)
        {
            isBusy = false;
            return default(T);
        }
#pragma warning restore CS0168 // Variable is declared but never used
    }

    public static async Task<string> Open(PopUpSheet page)
    {
        if (isBusy)
            return null;
        isBusy = true;
#pragma warning disable CS0168 // Variable is declared but never used
        try
        {
            if (Application.Current?.MainPage != null)
            {
                await page.BeforeOpen();
                await Application.Current.MainPage.Navigation.PushModalAsync(page, false);
                await page.AfterOpen();
            }

            return (string)await page.CallBackResult.Task;
        }
        catch (Exception ex)
        {
            isBusy = false;
            return "";
        }
#pragma warning restore CS0168 // Variable is declared but never used
    }

    public static async Task Close(object returnValue = null)
    {
        if (Application.Current?.MainPage != null && Application.Current.MainPage.Navigation.ModalStack.Count > 0)
        {
            PopUpSheet currentPage = (PopUpSheet)Application.Current.MainPage.Navigation.ModalStack.LastOrDefault();

            await currentPage.BeforeClose();
            if (currentPage.IsFadeBackground)
                await currentPage.BackgroundBack.FadeTo(0, 400);

            currentPage?.CallBackResult.TrySetResult(returnValue);
            await Application.Current.MainPage.Navigation.PopModalAsync(false);

            await currentPage.AfterClose();
            isBusy = false;
        }
    }
}
