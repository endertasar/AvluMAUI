using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Auth;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Auth;

public partial class SignupViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly TokenManager _tokens;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsStep1), nameof(IsStep2),
        nameof(StepTitle), nameof(StepSubtitle),
        nameof(Step2IndicatorColor))]
    private int _currentStep = 1;

    public bool   IsStep1              => CurrentStep == 1;
    public bool   IsStep2              => CurrentStep == 2;
    public string StepTitle            => CurrentStep == 1 ? "Site bilgileri"   : "Hesap bilgileri";
    public string StepSubtitle         => CurrentStep == 1 ? "Sitenizi sisteme tanıtalım." : "Yönetici hesabınızı oluşturun.";
    public Color  Step2IndicatorColor  => IsStep2
        ? Color.FromArgb("#0E2A47")
        : Color.FromArgb("#E4E8EF");

    // Step 1
    [ObservableProperty] private string  _siteUsername = string.Empty;
    [ObservableProperty] private string  _siteName     = string.Empty;
    [ObservableProperty] private string? _address;

    // Step 2
    [ObservableProperty] private string  _username         = string.Empty;
    [ObservableProperty] private string? _fullName;
    [ObservableProperty] private string  _password         = string.Empty;
    [ObservableProperty] private bool    _isPasswordHidden = true;

    // UI state
    [ObservableProperty] private bool    _isBusy       = false;
    [ObservableProperty] private string? _errorMessage;

    // Password strength
    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(StrengthBar1), nameof(StrengthBar2),
        nameof(StrengthBar3), nameof(StrengthBar4),
        nameof(PasswordStrengthLabel), nameof(IsPasswordStrengthVisible))]
    private int _passwordStrengthLevel;

    private static readonly Color SuccessColor  = Color.FromArgb("#2E7D57");
    private static readonly Color AccentColor   = Color.FromArgb("#C8A15A");
    private static readonly Color InactiveColor = Color.FromArgb("#E4E8EF");

    public Color  StrengthBar1              => PasswordStrengthLevel >= 1 ? SuccessColor : InactiveColor;
    public Color  StrengthBar2              => PasswordStrengthLevel >= 2 ? SuccessColor : InactiveColor;
    public Color  StrengthBar3              => PasswordStrengthLevel >= 3 ? AccentColor  : InactiveColor;
    public Color  StrengthBar4              => PasswordStrengthLevel >= 4 ? SuccessColor : InactiveColor;
    public bool   IsPasswordStrengthVisible => PasswordStrengthLevel > 0;
    public string PasswordStrengthLabel     => PasswordStrengthLevel switch
    {
        1 => "Çok Zayıf", 2 => "Zayıf", 3 => "Orta", 4 => "Güçlü", _ => string.Empty
    };

    public SignupViewModel(IAuthService auth, TokenManager tokens)
    {
        _auth   = auth;
        _tokens = tokens;
    }

    partial void OnPasswordChanged(string value) =>
        PasswordStrengthLevel = ComputeStrength(value);

    [RelayCommand]
    private async Task BackAsync()
    {
        if (CurrentStep == 2) { ErrorMessage = null; CurrentStep = 1; return; }
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private void NextStep()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(SiteUsername)) { ErrorMessage = "Site kullanıcı adı zorunludur."; return; }
        if (string.IsNullOrWhiteSpace(SiteName))     { ErrorMessage = "Site adı zorunludur."; return; }
        CurrentStep = 2;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Username))             { ErrorMessage = "Kullanıcı adı zorunludur."; return; }
        if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6) { ErrorMessage = "Şifre en az 6 karakter olmalıdır."; return; }

        IsBusy = true;
        try
        {
            var result = await _auth.RegisterAdminAsync(new AdminRegisterRequest
            {
                SiteUsername = SiteUsername.Trim(),
                SiteName     = SiteName.Trim(),
                Address      = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                Username     = Username.Trim(),
                Password     = Password,
                FullName     = string.IsNullOrWhiteSpace(FullName) ? null : FullName.Trim()
            });

            if (!result.Success || result.Data is null)
            {
                ErrorMessage = result.Message ?? "Kayıt başarısız.";
                return;
            }

            await _tokens.SetTokensAsync(result.Data.AccessToken, result.Data.RefreshToken);
            await NavigationHelper.GoToMainAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void TogglePassword() => IsPasswordHidden = !IsPasswordHidden;

    private static int ComputeStrength(string pw)
    {
        if (string.IsNullOrEmpty(pw)) return 0;
        int score = 0;
        if (pw.Length >= 6)                                score++;
        if (pw.Length >= 10)                               score++;
        if (pw.Any(char.IsUpper) && pw.Any(char.IsLower)) score++;
        if (pw.Any(c => !char.IsLetterOrDigit(c)))         score++;
        return Math.Clamp(score, 1, 4);
    }
}
