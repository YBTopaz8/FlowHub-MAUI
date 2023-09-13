using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Widget;
using AndroidX.Activity;
using System.Diagnostics;
using Activity = Android.App.Activity;

namespace FlowHub.Main;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(new[] { Platform.Intent.ActionAppAction },
    Categories = new[] { global::Android.Content.Intent.CategoryDefault})]
public class MainActivity : MauiAppCompatActivity
{

    protected override void OnCreate(Android.OS.Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        OnBackPressedDispatcher.AddCallback(this, new BackPress(this));
    }
    protected override void OnResume()
    {
        base.OnResume();
        Platform.OnResume(this);
    }

    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);
        Platform.OnNewIntent(intent);
    }

    private class BackPress : OnBackPressedCallback
    {
        private readonly Activity activity;
        private long backPressed;

        public BackPress(Activity activity) : base(true)
        {
            this.activity = activity;
        }

        public override void HandleOnBackPressed()
        {
            var navigation = Microsoft.Maui.Controls.Application.Current?.MainPage?.Navigation;
            if (navigation is not null && navigation.NavigationStack.Count <= 1 && navigation.ModalStack.Count <= 0)
            {
                const int delay = 2000;
                if (backPressed + delay > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                {
                    
                    activity.FinishAndRemoveTask();
                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                    
                    
                }
                else
                {
                    Android.Widget.Toast.MakeText(activity, "Close", ToastLength.Long)?.Show();
                    backPressed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }
        }
    }
}
