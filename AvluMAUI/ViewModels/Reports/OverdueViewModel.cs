using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AvluMAUI.Models.Report;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Reports;

public partial class OverdueViewModel : ObservableObject
{
    private readonly IReportService _reports;

    [ObservableProperty] private bool    _isBusy        = false;
    [ObservableProperty] private decimal _totalOverdue  = 0;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<OverdueChargeDto> Items { get; } = [];

    public OverdueViewModel(IReportService reports)
    {
        _reports = reports;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy       = true;
        ErrorMessage = null;
        Items.Clear();
        try
        {
            var result = await _reports.GetOverdueChargesAsync();
            if (!result.Success || result.Data is null) { ErrorMessage = result.Message; return; }
            foreach (var item in result.Data.OrderByDescending(x => x.DaysOverdue)) Items.Add(item);
            TotalOverdue = Items.Sum(x => x.RemainingAmount);
        }
        finally { IsBusy = false; }
    }
}
