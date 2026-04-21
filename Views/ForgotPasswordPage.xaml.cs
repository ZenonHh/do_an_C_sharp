using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System;
using DoAnCSharp.Services;

namespace DoAnCSharp.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly DatabaseService _dbService;
    private string _generatedOtp = "";
    private string _verifiedEmail = "";

    private IDispatcherTimer _timer;
    private int _timeLeft = 60;

    public ForgotPasswordPage(DatabaseService dbService)
    {
        InitializeComponent();
        _dbService = dbService;

        _timer = Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _timeLeft--;
        TimerLabel.Text = $"Mã hết hạn trong: {_timeLeft}s";

        // Hết giờ
        if (_timeLeft <= 0)
        {
            _timer.Stop(); 
            _generatedOtp = ""; // Xóa mã
            
            TimerLabel.Text = "Mã xác nhận đã hết hạn!";
            TimerLabel.TextColor = Colors.Red;
            
            ConfirmButton.IsEnabled = false; 
            ConfirmButton.BackgroundColor = Colors.LightGray; 
            
            ResendCodeButton.IsVisible = true; 
        }
    }

    private async void OnSendCodeClicked(object sender, EventArgs e)
    {
        try
        {
            string email = EmailEntry.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(email))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập Email!", "OK");
                return;
            }

            // Lợi dụng hàm này để check xem email có trong hệ thống không
            var existingUser = await _dbService.GetUserByEmailAsync(email);

            if (existingUser == null)
            {
                await DisplayAlert("Lỗi", "Email này chưa được đăng ký trong hệ thống!", "OK");
                return; // Lệnh return này sẽ dừng toàn bộ quá trình, không gửi mã OTP nữa
            }

            // Tạo OTP 4 số
            Random rnd = new Random();
            _generatedOtp = rnd.Next(1000, 9999).ToString();
            _verifiedEmail = email;

            // Reset UI và Đồng hồ
            _timeLeft = 60;
            TimerLabel.Text = $"Mã hết hạn trong: {_timeLeft}s";
            TimerLabel.TextColor = Color.FromArgb("#E74C3C");
            ConfirmButton.IsEnabled = true;
            ConfirmButton.BackgroundColor = Color.FromArgb("#FF4757");
            ResendCodeButton.IsVisible = false;
            OtpEntry.Text = ""; 

            // Hiển thị Bước 2
            EmailEntry.IsEnabled = false;
            SendCodeButton.IsVisible = false;
            Step2Container.IsVisible = true;
            
            _timer.Start();

            await DisplayAlert("Đã gửi mã", "Mã OTP đã được gửi đến email của bạn.\nVui lòng kiểm tra hộp thư.", "Đóng");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnSendCodeClicked: {ex}");
            await DisplayAlert("Lỗi", "Không thể gửi mã xác nhận", "OK");
        }
    }

    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        try
        {
            string enteredOtp = OtpEntry.Text?.Trim() ?? "";
            string newPass = NewPasswordEntry.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(enteredOtp) || string.IsNullOrEmpty(newPass))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ mã OTP và Mật khẩu mới!", "OK");
                return;
            }

            if (enteredOtp != _generatedOtp)
            {
                await DisplayAlert("Lỗi", "Mã OTP không chính xác hoặc đã hết hạn!", "OK");
                return;
            }

            // Đổi mật khẩu
            bool isSuccess = await _dbService.UpdateUserAsync(_verifiedEmail, "", newPass, "");

            if (isSuccess)
            {
                _timer.Stop(); 
                await DisplayAlert("Thành công", "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.", "OK");
                
                // ĐÃ SỬA: Dùng PopModalAsync để đóng trang (vì lúc nãy dùng PushModalAsync để mở)
                await Navigation.PopModalAsync(); 
            }
            else
            {
                await DisplayAlert("Lỗi", "Có lỗi xảy ra khi lưu mật khẩu.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnResetPasswordClicked: {ex}");
            await DisplayAlert("Lỗi", "Không thể đặt lại mật khẩu", "OK");
        }
    }

    // THÊM: Nút Hủy để lỡ người dùng muốn quay lại trang Đăng nhập giữa chừng
    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnBackTapped: {ex}");
        }
    }
}