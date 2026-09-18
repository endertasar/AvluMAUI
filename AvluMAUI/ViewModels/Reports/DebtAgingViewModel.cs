using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Models.Report;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Reports;

public partial class DebtAgingViewModel : ObservableObject
{
    private readonly IReportService _reports;

    [ObservableProperty] private bool          _isBusy = false;
    [ObservableProperty] private DebtAgingDto? _data;
    [ObservableProperty] private string?       _errorMessage;

    public DebtAgingViewModel(IReportService reports)
    {
        _reports = reports;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy       = true;
        ErrorMessage = null;
        try
        {
            var result = await _reports.GetDebtAgingAsync();
            if (!result.Success) { ErrorMessage = result.Message; return; }
            Data = result.Data;
        }
        finally { IsBusy = false; }
    }
}
