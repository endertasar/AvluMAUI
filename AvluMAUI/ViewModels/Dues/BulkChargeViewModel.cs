using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Dues;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Dues;

public partial class BulkChargeViewModel : ObservableObject
{
    private readonly IDuesService   _dues;
    private readonly IChargeService _charges;

    [ObservableProperty] private bool    _isBusy  = false;
    [ObservableProperty] private int     _year    = DateTime.Today.Year;
    [ObservableProperty] private int     _month   = DateTime.Today.Month;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string? _resultMessage;

    public ObservableCollection<DuesDefinition> Definitions    { get; } = [];
    public ObservableCollection<int>            AvailableYears { get; } = [];
    public ObservableCollection<int>            Months         { get; } = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

    private DuesDefinition? _selectedDefinition;
    public DuesDefinition? SelectedDefinition
    {
        get => _selectedDefinition;
        set => SetProperty(ref _selectedDefinition, value);
    }

    public BulkChargeViewModel(IDuesService dues, IChargeService charges)
    {
        _dues    = dues;
        _charges = charges;
        for (var y = DateTime.Today.Year - 1; y <= DateTime.Today.Year + 1; y++)
            AvailableYears.Add(y);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        Definitions.Clear();
        try
        {
            var result = await _dues.GetDefinitionsAsync();
            if (result.Success && result.Data is not null)
                foreach (var d in result.Data) Definitions.Add(d);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        await AlertHelper.ShowSuccessAsync(
            "Toplu aidat oluşturma otomatik olarak ayın başında çalışır. Manuel tetikleme bu sürümde desteklenmemektedir.");
        await NavigationHelper.GoBackAsync();
    }

    [RelayCommand]
    private Task CancelAsync() => NavigationHelper.GoBackAsync();
}
