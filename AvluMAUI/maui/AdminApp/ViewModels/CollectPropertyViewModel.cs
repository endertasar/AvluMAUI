using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Models.Property;
using AvluMAUI.Services.Interfaces;

namespace Avlu.AdminApp.ViewModels;

// Debt satırı — mülk + borç toplamı
public class DebtorItem
{
    public long   PropertyId  { get; init; }
    public string BlockUnit   { get; init; } = string.Empty;
    public string TitleLine   { get; init; } = string.Empty;
    public string Owner       { get; init; } = string.Empty;
    public string DebtDisplay { get; init; } = string.Empty;
}

public partial class CollectPropertyViewModel : ObservableObject
{
    private readonly IPropertyService _propertyService;

    [ObservableProperty] private bool            _isLoading;
    [ObservableProperty] private string?         _errorMessage;
    [ObservableProperty] private List<DebtorItem> _debtors = [];

    public CollectPropertyViewModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [RelayCommand]
    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var result = await _propertyService.GetAllAsync(ct);
            if (result.Success && result.Data is not null)
            {
                // Sadece aktif mülkleri listele; gerçek borç toplamı için charges endpoint gerekir.
                // Burada basit liste gösterimi yapılır; detay sayfası borcu yükler.
                Debtors = result.Data
                    .Where(p => p.IsActive)
                    .Select(p => new DebtorItem
                    {
                        PropertyId  = p.Id,
                        BlockUnit   = BuildBlockUnit(p),
                        TitleLine   = BuildTitleLine(p),
                        Owner       = p.OwnerName ?? p.ResidentName ?? "—",
                        DebtDisplay = "—"
                    }).ToList();
            }
            else
            {
                ErrorMessage = result.Message ?? "Mülkler yüklenemedi.";
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

    private static string BuildBlockUnit(PropertyDto p)
    {
        var b = p.Block?.Length > 0 ? p.Block[0].ToString() : "?";
        return $"{b}{p.UnitNo}";
    }

    private static string BuildTitleLine(PropertyDto p) =>
        string.IsNullOrEmpty(p.Block) ? p.UnitNo : $"{p.Block} · {p.UnitNo}";
}
