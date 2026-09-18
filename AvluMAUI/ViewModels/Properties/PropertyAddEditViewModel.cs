using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Property;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Properties;

[QueryProperty(nameof(PropertyId), "propertyId")]
public partial class PropertyAddEditViewModel : ObservableObject
{
    private readonly IPropertyService _properties;

    [ObservableProperty] private long    _propertyId;
    [ObservableProperty] private bool    _isBusy          = false;
    [ObservableProperty] private bool    _isEdit          = false;
    [ObservableProperty] private string  _block           = string.Empty;
    [ObservableProperty] private string  _unitNo          = string.Empty;
    [ObservableProperty] private string  _type            = "Daire";
    [ObservableProperty] private string? _ownerName;
    [ObservableProperty] private string? _ownerPhone;
    [ObservableProperty] private string? _residentName;
    [ObservableProperty] private string? _residentPhone;
    [ObservableProperty] private string  _duesResponsible = "Owner";
    [ObservableProperty] private bool    _isActive        = true;
    [ObservableProperty] private string? _errorMessage;

    public List<string> PropertyTypes    { get; } = ["Daire", "Dukkan", "Otopark"];
    public List<string> ResponsibleTypes { get; } = ["Owner", "Resident"];
    public string Title => IsEdit ? "Mülkü Düzenle" : "Yeni Mülk";

    public PropertyAddEditViewModel(IPropertyService properties)
    {
        _properties = properties;
    }

    partial void OnPropertyIdChanged(long value) => _ = LoadForEditAsync();

    private async Task LoadForEditAsync()
    {
        if (PropertyId == 0) return;
        IsEdit = true;
        IsBusy = true;
        try
        {
            var result = await _properties.GetByIdAsync(PropertyId);
            if (result.Success && result.Data is not null)
            {
                Block           = result.Data.Block          ?? string.Empty;
                UnitNo          = result.Data.UnitNo;
                Type            = result.Data.Type;
                OwnerName       = result.Data.OwnerName;
                OwnerPhone      = result.Data.OwnerPhone;
                ResidentName    = result.Data.ResidentName;
                ResidentPhone   = result.Data.ResidentPhone;
                DuesResponsible = result.Data.DuesResponsible;
                IsActive        = result.Data.IsActive;
            }
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(UnitNo))
        {
            ErrorMessage = "Birim no zorunludur.";
            return;
        }
        IsBusy       = true;
        ErrorMessage = null;
        try
        {
            if (IsEdit)
            {
                var req    = new UpdatePropertyRequest
                {
                    Block           = string.IsNullOrWhiteSpace(Block) ? null : Block,
                    UnitNo          = UnitNo,
                    Type            = Type,
                    OwnerName       = OwnerName,
                    OwnerPhone      = OwnerPhone,
                    ResidentName    = ResidentName,
                    ResidentPhone   = ResidentPhone,
                    DuesResponsible = DuesResponsible,
                    IsActive        = IsActive
                };
                var result = await _properties.UpdateAsync(PropertyId, req);
                if (!result.Success) { ErrorMessage = result.Message; return; }
            }
            else
            {
                var req    = new CreatePropertyRequest
                {
                    Block           = string.IsNullOrWhiteSpace(Block) ? null : Block,
                    UnitNo          = UnitNo,
                    Type            = Type,
                    OwnerName       = OwnerName,
                    OwnerPhone      = OwnerPhone,
                    ResidentName    = ResidentName,
                    ResidentPhone   = ResidentPhone,
                    DuesResponsible = DuesResponsible
                };
                var result = await _properties.CreateAsync(req);
                if (!result.Success) { ErrorMessage = result.Message; return; }
            }

            await AlertHelper.ShowSuccessAsync(IsEdit ? "Mülk güncellendi." : "Mülk eklendi.");
            await NavigationHelper.GoBackAsync();
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private Task CancelAsync() => NavigationHelper.GoBackAsync();
}
