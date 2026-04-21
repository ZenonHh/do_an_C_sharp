using DoAnCSharp.Services;
using DoAnCSharp.ViewModels;

namespace DoAnCSharp.Views;

public partial class PaymentPage : ContentPage
{
    public ILanguageService Lang { get; }

    public PaymentPage(PaymentViewModel viewModel, ILanguageService langService)
    {
        InitializeComponent();
        Lang = langService;
        BindingContext = viewModel;

#if DEBUG
        DevResetButton.IsVisible = true;
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        OnPropertyChanged(nameof(Lang));
        if (BindingContext is PaymentViewModel vm)
            vm.Refresh();
    }
}
