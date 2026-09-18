using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AvluMAUI.Models.Report;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Reports;

public partial class PropertySummaryViewModel : ObservableObject
{
    private readonly IReportService _reports;

    [ObservableProperty] private bool    _isBusy = false;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<PropertySummaryDto> Items { get; } = [];

    public PropertySummaryViewModel(IReportService reports)
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
            var result = await _reports.GetPropertySummaryAsync();
            if (!result.Success || result.Data is null) { ErrorMessage = result.Message; return; }
            foreach (var item in result.Data.OrderByDescending(x => x.Balance)) Items.Add(item);
        }
        finally { IsBusy = false; }
    }
}
