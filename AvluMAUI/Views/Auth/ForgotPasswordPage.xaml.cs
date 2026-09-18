using AvluMAUI.ViewModels.Auth;

namespace AvluMAUI.Views.Auth;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage(ForgotPasswordViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
