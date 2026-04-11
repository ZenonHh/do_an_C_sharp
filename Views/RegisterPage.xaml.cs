using DoAnCSharp.ViewModels;
using Microsoft.Maui.Controls;
using System;

namespace DoAnCSharp.Views;

public partial class RegisterPage : ContentPage
{
    private readonly RegisterViewModel _viewModel;

    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    // ĐÃ FIX: Thêm hàm này để XAML không bị lỗi văng app
    private async void OnAvatarClicked(object sender, EventArgs e)
    {
        try
        {
            string action = await DisplayActionSheet("Hình đại diện", "Hủy", null, "📷 Chụp ảnh mới", "🖼️ Chọn từ thư viện ảnh");

            if (action == "📷 Chụp ảnh mới")
            {
                _viewModel.SelectAvatarSourceCommand.Execute("camera");
            }
            else if (action == "🖼️ Chọn từ thư viện ảnh")
            {
                _viewModel.SelectAvatarSourceCommand.Execute("library");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnAvatarClicked: {ex}");
            await DisplayAlert("Lỗi", "Không thể chọn ảnh đại diện", "OK");
        }
    }
}