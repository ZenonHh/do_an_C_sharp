using ZXing.Net.Maui;
using CommunityToolkit.Mvvm.Messaging;
using DoAnCSharp.Models;
using DoAnCSharp.Services;

namespace DoAnCSharp.Views
{
    public partial class ScanQRPage : ContentPage
    {
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
                string? poiName = ExtractPoiName(scannedValue);

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
        // 3. Tên POI thuần (nếu không phải URL)
        private static string? ExtractPoiName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                // Deep link scheme hoặc web URL
                if (uri.Scheme == "vinhkhanhtour" || uri.Scheme == "http" || uri.Scheme == "https")
                {
                    string query = uri.Query.TrimStart('?');
                    foreach (var pair in query.Split('&'))
                    {
                        var parts = pair.Split('=');
                        if (parts.Length == 2 && Uri.UnescapeDataString(parts[0]) == "poi_name")
                            return Uri.UnescapeDataString(parts[1]);
                    }
                }
                return null; // URL không có poi_name
            }

            // Không phải URL → coi là tên POI thuần
            return value.Trim();
        }
    }
}
