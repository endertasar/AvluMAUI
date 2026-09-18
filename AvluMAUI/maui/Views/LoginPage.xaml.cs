using Microsoft.Maui.Controls;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Auth;
using AvluMAUI.Services.Interfaces;

namespace Avlu.Views;

public partial class LoginPage : ContentPage
{
    private readonly IAuthService _authService;
    private readonly TokenManager _tokenManager;
    private string _role = "resident";
    private string _lang = "TR";

    public LoginPage(IAuthService authService, TokenManager tokenManager)
    {
        _authService  = authService;
        _tokenManager = tokenManager;
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
        var navy      = (Color)(Application.Current?.Resources["Navy"] ?? Colors.Navy);
        var sub       = (Color)(Application.Current?.Resources["Sub"]  ?? Colors.Gray);

        RoleResidentBtn.BackgroundColor = role == "resident" ? active : Colors.Transparent;
        RoleResidentBtn.TextColor       = role == "resident" ? navy   : sub;
        RoleManagerBtn.BackgroundColor  = role == "manager"  ? active : Colors.Transparent;
        RoleManagerBtn.TextColor        = role == "manager"  ? navy   : sub;

        PhoneRow.IsVisible       = role == "resident";
        AdminFieldsRow.IsVisible = role == "manager";
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
        if (_role == "manager")
            await LoginAdminAsync();
        else
            await DisplayAlert("Yakında", "Mülk sahibi girişi ResidentApp'te açılacak.", "Tamam");
    }

    private async Task LoginAdminAsync()
    {
        var siteUsername = SiteUsernameEntry.Text?.Trim() ?? string.Empty;
        var username     = UsernameEntry.Text?.Trim() ?? string.Empty;
        var password     = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrEmpty(siteUsername) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Hata", "Lütfen tüm alanları doldurun.", "Tamam");
            return;
        }

        try
        {
            var result = await _authService.LoginAdminAsync(new AdminLoginRequest
            {
                SiteUsername = siteUsername,
                Username     = username,
                Password     = password
            });

            if (result.Success && result.Data is not null)
            {
                await _tokenManager.SetTokensAsync(result.Data.AccessToken, result.Data.RefreshToken);
                ((App)Application.Current!).GoToAdminShell();
            }
            else
            {
                await DisplayAlert("Giriş Başarısız", result.Message ?? "Kullanıcı adı veya şifre hatalı.", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }
}
