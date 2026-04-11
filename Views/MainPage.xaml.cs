using Microsoft.Maui.Controls;
using System;
using DoAnCSharp.Services; 

// Lưu ý: Nếu App báo lỗi namespace không khớp XAML, hãy đổi thành "namespace DoAnCSharp;"
namespace DoAnCSharp.Views;

public partial class MainPage : Shell
{
    // Chỉ khai báo ĐÚNG 1 LẦN duy nhất ở đây
    private readonly ILanguageService _languageService;

    public MainPage(ILanguageService languageService)
    {
        InitializeComponent();
        _languageService = languageService;
    }
}