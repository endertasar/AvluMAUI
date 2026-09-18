using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Models.Dues;
using AvluMAUI.Models.Property;
using AvluMAUI.Services.Interfaces;

namespace Avlu.AdminApp.ViewModels;

public class ChargeItem
{
    public long    ChargeId   { get; init; }
    public string  TargetType { get; init; } = "Dues";
    public string  Title      { get; init; } = string.Empty;
    public decimal Remaining  { get; init; }
    public string  Status     { get; init; } = string.Empty;
    public bool    IsOverdue  { get; init; }

    public string StatusLabel => Status switch
    {
        "Pending" => "Bekliyor",
        "Partial" => "Kısmi",
        _         => Status
    };

    public string RemainingDisplay => $"{Remaining:N2} ₺";
}

[QueryProperty(nameof(PropertyId), "propertyId")]
public partial class CollectDebtViewModel : ObservableObject
{
    private readonly IPropertyService _propertyService;
    private readonly IChargeService   _chargeService;

    [ObservableProperty] private long             _propertyId;
    [ObservableProperty] private bool             _isLoading;
    [ObservableProperty] private string?          _errorMessage;
    [ObservableProperty] private PropertyDto?     _property;
    [ObservableProperty] private List<ChargeItem> _charges = [];
    [ObservableProperty] private decimal          _totalDebt;

    public CollectDebtViewModel(IPropertyService propertyService, IChargeService chargeService)
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
                    IsOverdue  = c.DueDate.HasValue && c.DueDate.Value < DateTime.UtcNow && c.Status != "Paid"
                }).ToList();
                TotalDebt = Charges.Sum(c => c.Remaining);
            }
            else
            {
                ErrorMessage = chargeResult.Message ?? "Borçlar yüklenemedi.";
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
