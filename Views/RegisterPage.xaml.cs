using DoAnCSharp.ViewModels;
using Microsoft.Maui.Controls;

namespace DoAnCSharp.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}