using AvluMAUI.ViewModels.Auth;

namespace AvluMAUI.Views.Auth;

public partial class SignupPage : ContentPage
{
    public SignupPage(SignupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
