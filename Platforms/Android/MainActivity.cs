using Android.App;
using Android.Content.PM;
using Android.OS;

// Lưu ý: Namespace này phải trùng với tên Project của bạn
namespace DoAnCSharp;

// ĐẢM BẢO CÓ CHỮ MainLauncher = true Ở ĐÂY
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}