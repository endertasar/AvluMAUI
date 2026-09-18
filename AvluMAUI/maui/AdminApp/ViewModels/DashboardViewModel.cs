using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Models.Report;
using AvluMAUI.Services.Interfaces;

namespace Avlu.AdminApp.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IReportService _reportService;

    [ObservableProperty] private bool        _isLoading;
    [ObservableProperty] private string?     _errorMessage;
    [ObservableProperty] private DashboardDto? _dashboard;
    [ObservableProperty] private string      _periodLabel = string.Empty;

    public DashboardViewModel(IReportService reportService)
    {
        _reportService = reportService;
        var now = DateTime.Now;
        PeriodLabel = now.ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));
    }

    [RelayCommand]
    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var result = await _reportService.GetDashboardAsync(ct);
            if (result.Success && result.Data is not null)
                Dashboard = result.Data;
            else
                ErrorMessage = result.Message ?? "Veriler yüklenemedi.";
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
