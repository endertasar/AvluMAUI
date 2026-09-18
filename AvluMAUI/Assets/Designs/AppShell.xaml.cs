using Avlu.Views;
using Microsoft.Maui.Controls;

namespace Avlu;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("signup", typeof(SignupPage));
        Routing.RegisterRoute("forgot", typeof(ForgotPasswordPage));
        Routing.RegisterRoute("otp",    typeof(OtpPage));
    }
}
