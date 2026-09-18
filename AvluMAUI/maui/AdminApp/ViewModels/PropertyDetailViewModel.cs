using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Models.Property;
using AvluMAUI.Services.Interfaces;

namespace Avlu.AdminApp.ViewModels;

[QueryProperty(nameof(PropertyId), "propertyId")]
public partial class PropertyDetailViewModel : ObservableObject
{
    private readonly IPropertyService _propertyService;
    private readonly IChargeService   _chargeService;

    [ObservableProperty] private long             _propertyId;
    [ObservableProperty] private bool             _isLoading;
    [ObservableProperty] private string?          _errorMessage;
    [ObservableProperty] private PropertyDto?     _property;
    [ObservableProperty] private List<ChargeItem> _charges = [];
    [ObservableProperty] private decimal          _totalDebt;

    public PropertyDetailViewModel(IPropertyService propertyService, IChargeService chargeService)
    {
        _propertyService = propertyService;
        _chargeService   = chargeService;
    }

    [RelayCommand]
    public async Task LoadAsync(CancellationToken ct = default)
    {
        if (PropertyId <= 0) return;
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var propResult = await _propertyService.GetByIdAsync(PropertyId, ct);
            if (!propResult.Success || propResult.Data is null)
            {
                ErrorMessage = propResult.Message ?? "Mülk bulunamadı.";
                return;
            }
            Property = propResult.Data;

            var chargeResult = await _chargeService.GetByPropertyAsync(PropertyId, ct);
            if (chargeResult.Success && chargeResult.Data is not null)
            {
                var open = chargeResult.Data.Where(c => c.Status != "Paid").ToList();
                Charges = open.Select(c => new ChargeItem
                {
                    ChargeId   = c.Id,
                    TargetType = "Dues",
                    Title      = $"{c.PeriodLabel} Aidatı",
                    Remaining  = c.Amount - c.PaidAmount,
                    Status     = c.Status,
                    IsOverdue  = c.DueDate.HasValue && c.DueDate.Value < DateTime.UtcNow
                }).ToList();
                TotalDebt = Charges.Sum(c => c.Remaining);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
