using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Property;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Properties;

[QueryProperty(nameof(PropertyId), "propertyId")]
public partial class AssignResidentViewModel : ObservableObject
{
    private readonly IPropertyService _properties;

    [ObservableProperty] private long    _propertyId;
    [ObservableProperty] private bool    _isBusy          = false;
    [ObservableProperty] private string? _ownerName;
    [ObservableProperty] private string? _ownerPhone;
    [ObservableProperty] private string? _residentName;
    [ObservableProperty] private string? _residentPhone;
    [ObservableProperty] private string  _duesResponsible = "Owner";
    [ObservableProperty] private string? _errorMessage;

    public List<string> ResponsibleTypes { get; } = ["Owner", "Resident"];

    public AssignResidentViewModel(IPropertyService properties)
    {
        _properties = properties;
    }

    partial void OnPropertyIdChanged(long value) => _ = LoadAsync();

    private async Task LoadAsync()
    {
        if (PropertyId == 0) return;
        IsBusy = true;
        try
        {
            var result = await _properties.GetByIdAsync(PropertyId);
            if (result.Success && result.Data is not null)
            {
                OwnerName       = result.Data.OwnerName;
                OwnerPhone      = result.Data.OwnerPhone;
                ResidentName    = result.Data.ResidentName;
                ResidentPhone   = result.Data.ResidentPhone;
                DuesResponsible = result.Data.DuesResponsible;
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        IsBusy       = true;
        ErrorMessage = null;
        try
        {
            var propResult = await _properties.GetByIdAsync(PropertyId);
            if (!propResult.Success || propResult.Data is null) { ErrorMessage = "Mülk bulunamadı."; return; }

            var p = propResult.Data;
            var req = new UpdatePropertyRequest
            {
                Block           = p.Block,
                UnitNo          = p.UnitNo,
                Type            = p.Type,
                OwnerName       = OwnerName,
                OwnerPhone      = OwnerPhone,
                ResidentName    = ResidentName,
                ResidentPhone   = ResidentPhone,
                DuesResponsible = DuesResponsible,
                IsActive        = p.IsActive
            };
            var result = await _properties.UpdateAsync(PropertyId, req);
            if (!result.Success) { ErrorMessage = result.Message; return; }
            await AlertHelper.ShowSuccessAsync("İletişim bilgileri güncellendi.");
            await NavigationHelper.GoBackAsync();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private Task CancelAsync() => NavigationHelper.GoBackAsync();
}
