using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Models.Property;
using AvluMAUI.Services.Interfaces;

namespace Avlu.AdminApp.ViewModels;

public partial class PropertyListViewModel : ObservableObject
{
    private readonly IPropertyService _propertyService;
    private List<PropertyDto> _all = [];

    [ObservableProperty] private bool                  _isLoading;
    [ObservableProperty] private string?               _errorMessage;
    [ObservableProperty] private List<PropertyDto>     _properties = [];
    [ObservableProperty] private string                _searchText  = string.Empty;
    [ObservableProperty] private string                _activeFilter = "Tümü";

    public string[] Filters { get; } = ["Tümü", "Daire", "Dükkan", "Otopark", "Borçlu"];

    public PropertyListViewModel(IPropertyService propertyService)
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
                _all = result.Data;
                ApplyFilter();
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

    public void SetSearch(string text)
    {
        SearchText = text;
        ApplyFilter();
    }

    public void SetFilter(string filter)
    {
        ActiveFilter = filter;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = SearchText.Trim().ToLowerInvariant();
        var filtered = _all.AsEnumerable();

        if (ActiveFilter != "Tümü")
        {
            filtered = ActiveFilter == "Borçlu"
                ? filtered // Borçlu filtresi şimdilik kaldırılacak (balance bilgisi ayrı endpoint)
                : filtered.Where(p => p.Type.Equals(ActiveFilter, StringComparison.OrdinalIgnoreCase)
                                   || p.TypeLabel.Equals(ActiveFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(q))
        {
            filtered = filtered.Where(p =>
                (p.Block?.ToLowerInvariant().Contains(q) ?? false) ||
                p.UnitNo.ToLowerInvariant().Contains(q) ||
                (p.OwnerName?.ToLowerInvariant().Contains(q) ?? false) ||
                (p.ResidentName?.ToLowerInvariant().Contains(q) ?? false));
        }

        Properties = [..filtered];
    }

    // Bir mülk satırında gösterilecek görünüm alanları
    public static string BlockUnit(PropertyDto p)
    {
        var block = p.Block?.Length > 0 ? p.Block[0].ToString() : "?";
        return $"{block}{p.UnitNo}";
    }

    public static string TitleLine(PropertyDto p) =>
        string.IsNullOrEmpty(p.Block) ? p.UnitNo : $"{p.Block} · {p.UnitNo}";

    public static string Owner(PropertyDto p) =>
        p.OwnerName ?? p.ResidentName ?? "—";
}
