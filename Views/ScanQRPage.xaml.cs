using ZXing.Net.Maui;
using CommunityToolkit.Mvvm.Messaging;
using DoAnCSharp.Models;
using DoAnCSharp.Services;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoAnCSharp.Views
{
    public partial class ScanQRPage : ContentPage
    {
        private static readonly Regex _poiPathRegex = new(@"^/POI_\w+$", RegexOptions.Compiled);
        private readonly DatabaseService _dbService;
        private bool _hasScanned = false;

        public ScanQRPage(DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;

            cameraBarcodeReaderView.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormat.QrCode,
                AutoRotate = true,
                Multiple = false
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _hasScanned = false;
            cameraBarcodeReaderView.IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            cameraBarcodeReaderView.IsDetecting = false;
            base.OnDisappearing();
        }

        private void CameraBarcodeReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (_hasScanned) return;
            var result = e.Results?.FirstOrDefault();
            if (result == null) return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                _hasScanned = true;
                cameraBarcodeReaderView.IsDetecting = false;

                string scannedValue = result.Value;

                if (IsFoodStreetQR(scannedValue))
                {
                    await HandleFoodStreetScanAsync();
                    return;
                }

                string? poiName = await ExtractPoiNameAsync(scannedValue);

                if (poiName != null)
                {
                    // Pop trước → MapPage.OnAppearing re-register handler → rồi mới gửi message
                    await Navigation.PopAsync();
                    await Task.Delay(300);
                    WeakReferenceMessenger.Default.Send(new QrScannedMessage(poiName));
                }
                else
                {
                    // QR không hợp lệ → hiện tải app
                    bool download = await DisplayAlert(
                        "Không nhận ra mã QR",
                        "Mã QR này không thuộc ứng dụng VinhKhanhFoodTour.\nBạn có muốn tải ứng dụng để trải nghiệm đầy đủ?",
                        "Tải ứng dụng", "Đóng");

                    if (download)
                        await Browser.Default.OpenAsync(
                            "https://play.google.com/store/apps/details?id=com.companyname.doancsharp_clean",
                            BrowserLaunchMode.SystemPreferred);

                    _hasScanned = false;
                    cameraBarcodeReaderView.IsDetecting = true;
                }
            });
        }

        // Nhận dạng mã QR của phố ẩm thực (khác với QR của từng quán ăn)
        private static bool IsFoodStreetQR(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            if (value.StartsWith("vinhkhanhtour://foodstreet", StringComparison.OrdinalIgnoreCase))
                return true;

            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                var last = uri.AbsolutePath.Split('/').LastOrDefault() ?? "";
                return last.StartsWith("FOODSTREET", StringComparison.OrdinalIgnoreCase);
            }

            return value.Trim().StartsWith("FOODSTREET", StringComparison.OrdinalIgnoreCase);
        }

        // Xử lý khi quét QR phố ẩm thực: tải danh sách quán → người dùng chọn → phát audio
        private async Task HandleFoodStreetScanAsync()
        {
            var pois = await _dbService.GetPOIsAsync();

            if (pois.Count == 0)
            {
                await DisplayAlert("Thông báo", "Chưa có dữ liệu quán ăn. Vui lòng thử lại sau.", "OK");
                _hasScanned = false;
                cameraBarcodeReaderView.IsDetecting = true;
                return;
            }

            string[] names = pois.Select(p => p.Name).ToArray();
            string selected = await DisplayActionSheet(
                "🍜 Phố Ẩm Thực Vĩnh Khánh\nChọn quán ăn muốn nghe thuyết minh",
                "Hủy", null,
                names);

            if (string.IsNullOrEmpty(selected) || selected == "Hủy")
            {
                _hasScanned = false;
                cameraBarcodeReaderView.IsDetecting = true;
                return;
            }

            await Navigation.PopAsync();
            await Task.Delay(300);
            WeakReferenceMessenger.Default.Send(new QrScannedMessage(selected));
        }

        // Trích xuất poi_name từ các định dạng QR hỗ trợ
        private async Task<string?> ExtractPoiNameAsync(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                // 1. Xử lý deep link chứa tham số poi_name
                string query = uri.Query.TrimStart('?');
                foreach (var pair in query.Split('&'))
                {
                    var parts = pair.Split('=');
                    if (parts.Length == 2 && Uri.UnescapeDataString(parts[0]) == "poi_name")
                        return Uri.UnescapeDataString(parts[1]);
                }

                // 2. Xử lý URL từ Web Admin (vd: .../qr/POI_ABC hoặc .../POI_ABC)
                bool isPOIUrl = (uri.Scheme == "http" || uri.Scheme == "https") &&
                                (uri.AbsolutePath.Contains("/qr/") ||
                                 _poiPathRegex.IsMatch(uri.AbsolutePath));
                if (isPOIUrl)
                {
                    try
                    {
                        string code = uri.AbsolutePath.Split('/').Last();
                        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                        string apiUrl = $"{uri.Scheme}://{uri.Authority}/api/pois/qr/{code}";
                        var response = await http.GetAsync(apiUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonString = await response.Content.ReadAsStringAsync();
                            var json = System.Text.Json.JsonDocument.Parse(jsonString);
                            if (json.RootElement.TryGetProperty("name", out var nameElement))
                                return nameElement.GetString();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"API QR Lookup Error: {ex.Message}");
                    }
                }
            }

            return value.Trim();
        }
    }
}
