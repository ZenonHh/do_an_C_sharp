using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace DoAnCSharp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "vinhkhanhtour",
    DataHost = "play_audio")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        // Xử lý deep link khi app khởi động lạnh (cold start)
        if (Intent?.Data != null &&
            Uri.TryCreate(Intent.Data.ToString(), UriKind.Absolute, out var uri))
        {
            Microsoft.Maui.Controls.Application.Current?.SendOnAppLinkRequestReceived(uri);
        }
    }

    // Xử lý deep link khi app đang chạy (foreground/background) — SingleTop ensures this is called
    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        if (intent?.Data != null &&
            Uri.TryCreate(intent.Data.ToString(), UriKind.Absolute, out var uri))
        {
            Microsoft.Maui.Controls.Application.Current?.SendOnAppLinkRequestReceived(uri);
        }
    }
}