using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AvluMAUI.Models.Report;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Reports;

public partial class MonthlyCollectionViewModel : ObservableObject
{
    private readonly IReportService _reports;

    [ObservableProperty] private bool    _isBusy = false;
    [ObservableProperty] private int     _year   = DateTime.Today.Year;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<MonthlyCollectionDto> Rows         { get; } = [];
    public ObservableCollection<int>                  AvailableYears { get; } = [];

    public MonthlyCollectionViewModel(IReportService reports)
    {
        _reports = reports;
        for (var y = DateTime.Today.Year - 2; y <= DateTime.Today.Year; y++)
            AvailableYears.Add(y);
    }

    partial void OnYearChanged(int value) => _ = LoadAsync();

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy       = true;
        ErrorMessage = null;
        Rows.Clear();
        try
        {
            var result = await _reports.GetMonthlyCollectionAsync(Year);
            if (!result.Success || result.Data is null) { ErrorMessage = result.Message; return; }
            foreach (var r in result.Data) Rows.Add(r);
        }
        finally { IsBusy = false; }
    }
}
