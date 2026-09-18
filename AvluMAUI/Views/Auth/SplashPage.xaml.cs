using AvluMAUI.Helpers;

namespace AvluMAUI.Views.Auth;

public partial class SplashPage : ContentPage
{
    private readonly TokenManager _tokens;

    public SplashPage(TokenManager tokens)
    {
        StartupLogger.Step("SplashPage ctor");
        InitializeComponent();
        _tokens = tokens;
        StartupLogger.Step("SplashPage ctor done");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        StartupLogger.Step("SplashPage.OnAppearing");

        await Task.Delay(1500);

        try
        {
            await _tokens.LoadSavedTokenAsync();
            StartupLogger.Step("Tokens loaded");
        }
        catch (Exception ex)
        {
            StartupLogger.Fail("LoadSavedTokenAsync", ex);
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                StartupLogger.Step($"Navigate: HasTenantToken={_tokens.HasTenantToken}");

                if (_tokens.HasTenantToken)
                    await NavigationHelper.GoToMainAsync();
                else
                    await Shell.Current.GoToAsync("//login");

                StartupLogger.Step("Navigation OK");
            }
            catch (Exception ex)
            {
                StartupLogger.Fail("Navigation", ex);
                try
                {
                    await Shell.Current.GoToAsync("//login");
                    StartupLogger.Step("Navigation fallback OK");
                }
                catch (Exception ex2)
                {
                    StartupLogger.Fail("Navigation fallback", ex2);
                }
            }
        });
    }
}
