using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace DoAnCSharp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "vinhkhanhtour",
    DataHost = "play_audio")]
public class MainActivity : MauiAppCompatActivity
{
    // Xử lý deep link khi app đang chạy (foreground/background)
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