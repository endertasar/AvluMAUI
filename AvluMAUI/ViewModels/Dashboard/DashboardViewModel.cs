using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Dues;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Dashboard;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IChargeService _charges;
    private readonly TokenManager   _tokens;

    [ObservableProperty] private bool              _isBusy        = false;
    [ObservableProperty] private ChargeSummaryDto? _summary;
    [ObservableProperty] private string?           _errorMessage;

    public DashboardViewModel(IChargeService charges, TokenManager tokens)
    {
        _charges = charges;
        _tokens  = tokens;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy       = true;
        ErrorMessage = null;
        try
        {
            var result = await _charges.GetSummaryAsync();
            if (!result.Success)
                ErrorMessage = result.Message;
            else
                Summary = result.Data;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirmed = await AlertHelper.ShowConfirmAsync("Çıkış yapmak istediğinizden emin misiniz?");
        if (confirmed)
            await _tokens.ClearAsync();
    }
}
