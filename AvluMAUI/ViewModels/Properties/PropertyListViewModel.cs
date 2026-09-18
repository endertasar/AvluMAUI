using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Property;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Properties;

public partial class PropertyListViewModel : ObservableObject
{
    private readonly IPropertyService _properties;

    [ObservableProperty] private bool    _isBusy = false;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private string  _searchText = string.Empty;

    public ObservableCollection<PropertyDto> Properties { get; } = [];

    private List<PropertyDto> _allProperties = [];

    public PropertyListViewModel(IPropertyService properties)
    {
        _properties = properties;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy       = true;
        ErrorMessage = null;
        Properties.Clear();
        try
        {
            var result = await _properties.GetAllAsync();
            if (!result.Success || result.Data is null)
            {
                ErrorMessage = result.Message ?? "Mülkler yüklenemedi.";
                return;
            }
            _allProperties = result.Data;
            foreach (var p in _allProperties) Properties.Add(p);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        Properties.Clear();
        var filtered = string.IsNullOrWhiteSpace(value)
            ? _allProperties
            : _allProperties.Where(p =>
                p.UnitNo.Contains(value, StringComparison.OrdinalIgnoreCase) ||
                (p.Block?.Contains(value, StringComparison.OrdinalIgnoreCase) == true) ||
                (p.OwnerName?.Contains(value, StringComparison.OrdinalIgnoreCase) == true));
        foreach (var p in filtered) Properties.Add(p);
    }

    [RelayCommand]
    private Task OpenDetailAsync(PropertyDto property) =>
        NavigationHelper.GoToPropertyDetailAsync(property.Id);

    [RelayCommand]
    private Task AddAsync() =>
        NavigationHelper.GoToPropertyAddEditAsync();
}
