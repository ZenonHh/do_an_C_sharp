using CommunityToolkit.Mvvm.ComponentModel;
using DoAnCSharp.Services;

namespace DoAnCSharp.ViewModels;

public partial class MapViewModel : ObservableObject
{
    // BẮT BUỘC PHẢI CÓ BIẾN NÀY ĐỂ GIAO DIỆN BINDING
    public ILanguageService Lang { get; }

    public MapViewModel(ILanguageService languageService)
    {
        Lang = languageService;
    }
}