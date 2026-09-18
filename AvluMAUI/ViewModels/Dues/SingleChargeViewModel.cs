using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Dues;
using AvluMAUI.Models.Property;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Dues;

[QueryProperty(nameof(PropertyId), "propertyId")]
public partial class SingleChargeViewModel : ObservableObject
{
    private readonly IChargeService   _charges;
    private readonly IPropertyService _properties;

    [ObservableProperty] private long    _propertyId;
    [ObservableProperty] private bool    _isBusy      = false;
    [ObservableProperty] private string  _amount      = string.Empty;
    [ObservableProperty] private string  _period      = DateTime.Today.ToString("yyyyMM");
    [ObservableProperty] private string  _dueDate     = DateTime.Today.AddDays(15).ToString("yyyy-MM-dd");
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<PropertyDto> Properties { get; } = [];

    private PropertyDto? _selectedProperty;
    public PropertyDto? SelectedProperty
    {
        get => _selectedProperty;
        set { if (SetProperty(ref _selectedProperty, value) && value is not null) PropertyId = value.Id; }
    }

    public SingleChargeViewModel(IChargeService charges, IPropertyService properties)
    {
        _charges    = charges;
        _properties = properties;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        Properties.Clear();
        try
        {
            var propResult = await _properties.GetAllAsync();
            if (propResult.Success && propResult.Data is not null)
                foreach (var p in propResult.Data) Properties.Add(p);

            if (PropertyId > 0)
                SelectedProperty = Properties.FirstOrDefault(p => p.Id == PropertyId);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedProperty is null || !decimal.TryParse(Amount, out var amount) || amount <= 0)
        {
            ErrorMessage = "Mülk ve geçerli tutar zorunludur.";
            return;
        }
        if (!int.TryParse(Period, out var periodInt) || periodInt < 200001)
        {
            ErrorMessage = "Dönem YYYYMM formatında olmalıdır (örn: 202507).";
            return;
        }
        IsBusy       = true;
        ErrorMessage = null;
        try
        {
            DateTime? dueDateVal = DateTime.TryParse(DueDate, out var dt) ? dt.ToUniversalTime() : null;
            var req = new CreateChargeRequest
            {
                PropertyId = SelectedProperty.Id,
                Period     = periodInt,
                Amount     = amount,
                DueDate    = dueDateVal
            };
            var result = await _charges.SingleChargeAsync(req);
            if (!result.Success) { ErrorMessage = result.Message; return; }
            await AlertHelper.ShowSuccessAsync("Borç eklendi.");
            await NavigationHelper.GoBackAsync();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private Task CancelAsync() => NavigationHelper.GoBackAsync();
}
