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
        private readonly ScanQuotaService _quotaService;
        private bool _hasScanned = false;

        public ScanQRPage(ScanQuotaService quotaService)
        {
            InitializeComponent();
            _quotaService = quotaService;

            cameraBarcodeReaderView.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormat.QrCode,
                AutoRotate = true,
                Multiple = false
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Kiểm tra quota trước khi cho phép quét
            if (_quotaService.GetRemaining() <= 0)
            {
                cameraBarcodeReaderView.IsDetecting = false;
                bool goPayment = await DisplayAlert(
                    "Hết lượt quét",
                    "Bạn đã dùng hết lượt quét QR miễn phí.\nMua thêm lượt để tiếp tục?",
                    "Mua ngay", "Hủy");

                if (goPayment)
                    await Shell.Current.GoToAsync("PaymentPage");

                await Navigation.PopAsync();
                return;
            }

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
                string? poiName = await ExtractPoiNameAsync(scannedValue);

                if (poiName != null)
                {
                    // App đã cài → phát audio trực tiếp qua messaging
                    _quotaService.TryUseOne();
                    int remaining = _quotaService.GetRemaining();

                    // Pop trước → MapPage.OnAppearing re-register handler → rồi mới gửi message
                    await Navigation.PopAsync();
                    await Task.Delay(300); // đợi OnAppearing của MapPage hoàn tất
                    WeakReferenceMessenger.Default.Send(new QrScannedMessage(poiName));

                    if (remaining <= 1)
                        await Shell.Current.DisplayAlert("Sắp hết lượt",
                            $"Bạn còn {remaining} lượt quét. Hãy mua thêm để không bị gián đoạn!", "OK");
                }
                else
                {
                    // QR không hợp lệ / không phải của ứng dụng → hiện tải app
                    bool download = await DisplayAlert(
                        "Không nhận ra mã QR",
                        "Mã QR này không thuộc ứng dụng VinhKhanhFoodTour.\nBạn có muốn tải ứng dụng để trải nghiệm đầy đủ?",
                        "Tải ứng dụng", "Đóng");

                    if (download)
                        await Browser.Default.OpenAsync(
                            "https://play.google.com/store/apps/details?id=com.companyname.doancsharp_clean",
                            BrowserLaunchMode.SystemPreferred);

                    // Reset để cho phép quét lại
                    _hasScanned = false;
                    cameraBarcodeReaderView.IsDetecting = true;
                }
            });
        }

        // Trích xuất poi_name từ các định dạng QR hỗ trợ:
        // 1. vinhkhanhtour://play_audio?poi_name=xxx
        // 2. https://any-domain.com/any-path?poi_name=xxx
        // 3. QR từ Web Admin: http://172.20.10.2:5000/qr/POI_XXX (Sẽ gọi API lấy tên)
        // 3. Tên POI thuần (nếu không phải URL)
        private async Task<string?> ExtractPoiNameAsync(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                // 1. Xử lý deep link cũ chứa tham số poi_name
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
                        string code = uri.AbsolutePath.Split('/').Last(); // Lấy "POI_ABC"
                        
                        // Gọi nhanh API của Web Admin để lấy tên Quán Ăn
                        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                        string apiUrl = $"{uri.Scheme}://{uri.Authority}/api/pois/qr/{code}";
                        
                        var response = await http.GetAsync(apiUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonString = await response.Content.ReadAsStringAsync();
                            var json = System.Text.Json.JsonDocument.Parse(jsonString);
                            if (json.RootElement.TryGetProperty("name", out var nameElement))
                            {
                                return nameElement.GetString(); // Trả về tên quán để App phát Audio
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"API QR Lookup Error: {ex.Message}");
                    }
                }
            }

            // Không phải URL → coi là tên POI thuần
            return value.Trim();
        }
    }
}
