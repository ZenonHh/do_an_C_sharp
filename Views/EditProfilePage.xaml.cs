using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using System;
using System.IO;
using System.Threading.Tasks;
using DoAnCSharp.Services;
using DoAnCSharp.Models;

namespace DoAnCSharp.Views;

public partial class EditProfilePage : ContentPage
{
    private string _newAvatarPath = "";
    private DatabaseService _dbService;
    private User? _currentUser;

    public EditProfilePage()
    {
        InitializeComponent();
        _dbService = new DatabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            // Load sẵn thông tin cũ lên UI
            _currentUser = await _dbService.GetCurrentUserAsync();
            if (_currentUser != null)
            {
                FullNameEntry.Text = _currentUser.FullName;
                ProfileImage.Source = _currentUser.Avatar;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in EditProfilePage.OnAppearing: {ex}");
            await DisplayAlert("Lỗi", "Không thể tải thông tin người dùng", "OK");
        }
    }

    private async void OnChangeAvatarClicked(object sender, EventArgs e)
    {
        try
        {
            // Mở thư viện ảnh điện thoại
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo != null)
            {
                _newAvatarPath = photo.FullPath;
                ProfileImage.Source = ImageSource.FromFile(_newAvatarPath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnChangeAvatarClicked: {ex}");
            await DisplayAlert("Lỗi", "Không thể mở thư viện ảnh", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            string newName = FullNameEntry.Text;
            string oldPass = OldPasswordEntry.Text;
            string newPass = NewPasswordEntry.Text;

            if (_currentUser == null) return;

            // Bắt buộc nhập mật khẩu cũ để xác minh
            if (oldPass != _currentUser.Password)
            {
                await DisplayAlert("Lỗi", "Mật khẩu hiện tại không đúng!", "OK");
                return;
            }

            // Gọi DB để lưu
            bool isSuccess = await _dbService.UpdateUserAsync(_currentUser.Email, newName, newPass, _newAvatarPath);
            
            if (isSuccess)
            {
                await DisplayAlert("Thành công", "Cập nhật thông tin hoàn tất!", "OK");
                await Navigation.PopAsync(); // Quay lại trang trước
            }
            else
            {
                await DisplayAlert("Lỗi", "Có lỗi xảy ra khi lưu trữ", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnSaveClicked: {ex}");
            await DisplayAlert("Lỗi", "Không thể lưu thông tin: " + ex.Message, "OK");
        }
    }
}