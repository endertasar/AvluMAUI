using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Dues;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Dues;

public partial class DuesDefinitionAddViewModel : ObservableObject
{
    private readonly IDuesService _dues;

    [ObservableProperty] private bool    _isBusy        = false;
    [ObservableProperty] private string  _propertyType  = "Daire";
    [ObservableProperty] private string  _amount        = string.Empty;
    [ObservableProperty] private string  _effectiveFrom = DateTime.Today.ToString("yyyyMM");
    [ObservableProperty] private string? _description;
    [ObservableProperty] private string? _errorMessage;

    public List<string> PropertyTypes { get; } = ["Daire", "Dukkan", "Otopark"];

    public DuesDefinitionAddViewModel(IDuesService dues)
    {
        _dues = dues;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!decimal.TryParse(Amount, out var amount) || amount <= 0)
        {
            ErrorMessage = "Geçerli bir tutar giriniz.";
            return;
        }
        if (!int.TryParse(EffectiveFrom, out var effectiveFrom) || effectiveFrom < 200001)
        {
            ErrorMessage = "Geçerlilik başlangıcını YYYYMM formatında giriniz (örn: 202507).";
            return;
        }
        IsBusy       = true;
        ErrorMessage = null;
        try
        {
            var req    = new CreateDuesDefinitionRequest
            {
                PropertyType  = PropertyType,
                Amount        = amount,
                EffectiveFrom = effectiveFrom,
                Description   = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim()
            };
            var result = await _dues.CreateDefinitionAsync(req);
            if (!result.Success) { ErrorMessage = result.Message; return; }
            await AlertHelper.ShowSuccessAsync("Aidat tanımı eklendi.");
            await NavigationHelper.GoBackAsync();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private Task CancelAsync() => NavigationHelper.GoBackAsync();
}
