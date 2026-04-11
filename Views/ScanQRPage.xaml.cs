using Microsoft.Maui.Controls;
using ZXing.Net.Maui;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using DoAnCSharp.Models;

namespace DoAnCSharp.Views
{
    public partial class ScanQRPage : ContentPage
    {
        public ScanQRPage()
        {
            InitializeComponent();
            
            // Cấu hình quét mã QR
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
            cameraBarcodeReaderView.IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            cameraBarcodeReaderView.IsDetecting = false;
            base.OnDisappearing();
        }

        private void CameraBarcodeReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var result = e.Results?.FirstOrDefault();
            if (result != null)
            {
                // Dừng quét ngay khi bắt được mã
                cameraBarcodeReaderView.IsDetecting = false;
                
                Dispatcher.Dispatch(async () =>
                {
                    string qrValue = result.Value; 
                    
                    // Gửi tin nhắn chứa nội dung QR đi
                    WeakReferenceMessenger.Default.Send(new QrScannedMessage(qrValue));
                    
                    // Quay về trang bản đồ
                    await Navigation.PopAsync();
                });
            }
        }
    }
}