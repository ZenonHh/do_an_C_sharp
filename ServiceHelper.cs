using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace DoAnCSharp
{
    public static class ServiceHelper
    {
        public static T? GetService<T>() where T : class
        {
            return IPlatformApplication.Current?.Services.GetService(typeof(T)) as T;
        }
    }
}
