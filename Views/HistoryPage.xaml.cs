using DoAnCSharp.ViewModels;
using Microsoft.Maui.Controls;

namespace DoAnCSharp.Views;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;

    public HistoryPage(HistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.LoadHistoryAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in HistoryPage.OnAppearing: {ex}");
            await DisplayAlert("Lỗi", "Không thể tải lịch sử", "OK");
        }
    }
}