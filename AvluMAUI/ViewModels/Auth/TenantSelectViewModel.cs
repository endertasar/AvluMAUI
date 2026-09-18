using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Auth;
using AvluMAUI.Models.Tenant;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Auth;

public partial class TenantSelectViewModel : ObservableObject
{
    private readonly IAuthService  _auth;
    private readonly TokenManager  _tokens;

    [ObservableProperty] private bool    _isBusy       = false;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<TenantDto> Tenants { get; } = [];

    public TenantSelectViewModel(IAuthService auth, TokenManager tokens)
    {
        _auth   = auth;
        _tokens = tokens;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy       = true;
        ErrorMessage = null;
        Tenants.Clear();
        try
        {
            // Get all tenants in the pre-auth token
            var tenantIds = _tokens.GetTenantIdsFromPreAuth();
            var result    = await _auth.GetMyTenantsAsync();
            if (!result.Success || result.Data is null)
            {
                ErrorMessage = result.Message ?? "Siteler yüklenemedi.";
                return;
            }
            // Filter to tenants the user has access to
            foreach (var t in result.Data.Where(t => tenantIds.Contains(t.TenantId)))
                Tenants.Add(t);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectAsync(TenantDto tenant)
    {
        IsBusy = true;
        try
        {
            var result = await _auth.SelectTenantAsync(new TenantSelectRequest { TenantId = tenant.TenantId });
            if (!result.Success || result.Data is null)
            {
                await AlertHelper.ShowErrorAsync(result.Message ?? "Seçim başarısız.");
                return;
            }
            await _tokens.SetTenantTokenAsync(result.Data);
            await NavigationHelper.GoToMainAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
