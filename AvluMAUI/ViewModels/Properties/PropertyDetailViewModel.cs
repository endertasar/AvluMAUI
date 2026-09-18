using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Dues;
using AvluMAUI.Models.Property;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Properties;

[QueryProperty(nameof(PropertyId), "propertyId")]
public partial class PropertyDetailViewModel : ObservableObject
{
    private readonly IPropertyService _properties;
    private readonly IChargeService   _charges;

    [ObservableProperty] private long         _propertyId;
    [ObservableProperty] private bool         _isBusy = false;
    [ObservableProperty] private PropertyDto? _property;
    [ObservableProperty] private string?      _errorMessage;

    public ObservableCollection<DuesChargeDto> Charges { get; } = [];

    public PropertyDetailViewModel(IPropertyService properties, IChargeService charges)
    {
        _properties = properties;
        _charges    = charges;
    }

    partial void OnPropertyIdChanged(long value) => _ = LoadAsync();

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (PropertyId == 0) return;
        IsBusy       = true;
        ErrorMessage = null;
        Charges.Clear();
        try
        {
            var propTask    = _properties.GetByIdAsync(PropertyId);
            var chargesTask = _charges.GetByPropertyAsync(PropertyId);
            await Task.WhenAll(propTask, chargesTask);

            if (!propTask.Result.Success) { ErrorMessage = propTask.Result.Message; return; }
            Property = propTask.Result.Data;

            if (chargesTask.Result.Success && chargesTask.Result.Data is not null)
                foreach (var c in chargesTask.Result.Data.OrderByDescending(x => x.DueDate ?? DateTime.MinValue))
                    Charges.Add(c);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task EditAsync() =>
        NavigationHelper.GoToPropertyAddEditAsync(PropertyId);

    [RelayCommand]
    private Task AssignResidentAsync() =>
        NavigationHelper.GoToAssignResidentAsync(PropertyId);

    [RelayCommand]
    private Task AddChargeAsync() =>
        NavigationHelper.GoToSingleChargeAsync(PropertyId);

    [RelayCommand]
    private Task ShowPaymentsAsync() =>
        NavigationHelper.GoToPaymentListAsync(PropertyId);

    [RelayCommand]
    private async Task DeleteAsync()
    {
        var confirmed = await AlertHelper.ShowConfirmAsync("Bu mülkü silmek istediğinizden emin misiniz?", "Mülkü Sil");
        if (!confirmed) return;
        IsBusy = true;
        try
        {
            var result = await _properties.DeleteAsync(PropertyId);
            if (!result.Success)
                await AlertHelper.ShowErrorAsync(result.Message ?? "Silme başarısız.");
            else
            {
                await AlertHelper.ShowSuccessAsync("Mülk silindi.");
                await NavigationHelper.GoBackAsync();
            }
        }
        finally { IsBusy = false; }
    }
}
