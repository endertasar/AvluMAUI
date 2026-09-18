using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Dues;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Dues;

public partial class DuesDefinitionListViewModel : ObservableObject
{
    private readonly IDuesService _dues;

    [ObservableProperty] private bool    _isBusy = false;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<DuesDefinition> Definitions { get; } = [];

    public DuesDefinitionListViewModel(IDuesService dues)
    {
        _dues = dues;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        Definitions.Clear();
        try
        {
            var result = await _dues.GetDefinitionsAsync();
            if (!result.Success || result.Data is null)
            {
                ErrorMessage = result.Message ?? "Tanımlar yüklenemedi.";
                return;
            }
            foreach (var d in result.Data) Definitions.Add(d);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private Task AddAsync() =>
        NavigationHelper.GoToAddDuesDefinitionAsync();

    [RelayCommand]
    private async Task DeleteAsync(DuesDefinition def)
    {
        var confirmed = await AlertHelper.ShowConfirmAsync($"'{def.PropertyType}' aidat tanımını silmek istiyor musunuz?");
        if (!confirmed) return;
        IsBusy = true;
        try
        {
            var result = await _dues.DeleteDefinitionAsync(def.Id);
            if (!result.Success)
                await AlertHelper.ShowErrorAsync(result.Message ?? "Silme başarısız.");
            else
            {
                Definitions.Remove(def);
                await AlertHelper.ShowSuccessAsync("Tanım silindi.");
            }
        }
        finally { IsBusy = false; }
    }
}
