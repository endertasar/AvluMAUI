using Microsoft.Maui.Controls;

namespace Avlu.Views;

public partial class LoginPage : ContentPage
{
    private string _role = "resident"; // "resident" | "manager"
    private string _lang = "TR";       // "TR" | "EN"

    public LoginPage()
    {
        InitializeComponent();
    }

    // ─── Role segmented ───────────────────────────────────────
    private void OnRoleResident(object sender, System.EventArgs e) => SetRole("resident");
    private void OnRoleManager(object sender, System.EventArgs e)  => SetRole("manager");

    private void SetRole(string role)
    {
        _role = role;
        var activeBg  = (Color)(Application.Current?.Resources["Surface"]     ?? Colors.White);
        var activeBgD = (Color)(Application.Current?.Resources["SurfaceDark"] ?? Colors.Black);
        var active    = Application.Current?.RequestedTheme == AppTheme.Dark ? activeBgD : activeBg;

        var navy = (Color)(Application.Current?.Resources["Navy"] ?? Colors.Navy);
        var sub  = (Color)(Application.Current?.Resources["Sub"]  ?? Colors.Gray);

        RoleResidentBtn.BackgroundColor = role == "resident" ? active : Colors.Transparent;
        RoleResidentBtn.TextColor       = role == "resident" ? navy   : sub;
        RoleManagerBtn.BackgroundColor  = role == "manager"  ? active : Colors.Transparent;
        RoleManagerBtn.TextColor        = role == "manager"  ? navy   : sub;
    }

    // ─── Lang ─────────────────────────────────────────────────
    private void OnLangTr(object sender, System.EventArgs e) => SetLang("TR");
    private void OnLangEn(object sender, System.EventArgs e) => SetLang("EN");

    private void SetLang(string lang)
    {
        _lang = lang;
        var navy = (Color)(Application.Current?.Resources["Navy"] ?? Colors.Navy);
        var sub  = (Color)(Application.Current?.Resources["Sub"]  ?? Colors.Gray);

        LangTrBtn.BackgroundColor = lang == "TR" ? navy : Colors.Transparent;
        LangTrBtn.TextColor       = lang == "TR" ? Colors.White : sub;
        LangEnBtn.BackgroundColor = lang == "EN" ? navy : Colors.Transparent;
        LangEnBtn.TextColor       = lang == "EN" ? Colors.White : sub;
        // Gerçek uygulamada burada CultureInfo + AppResources.Culture set edilir.
    }

    // ─── Password visibility ──────────────────────────────────
    private void OnTogglePassword(object sender, System.EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
    }

    // ─── Navigation ───────────────────────────────────────────
    private async void OnForgotTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("forgot");

    private async void OnSignupTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("signup");

    private async void OnLoginTapped(object sender, System.EventArgs e)
    {
        // TODO: validate + AuthService.LoginAsync(PhoneEntry.Text, PasswordEntry.Text, _role)
        // Başarılıysa ana sayfaya, SMS gerekiyorsa OTP'ye:
        await Shell.Current.GoToAsync("otp");
    }
}
