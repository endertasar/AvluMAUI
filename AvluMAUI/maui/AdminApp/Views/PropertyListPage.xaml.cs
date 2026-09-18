using Microsoft.Maui.Controls;
using Avlu.AdminApp.ViewModels;
using Avlu.Controls;
using AvluMAUI.Models.Property;

namespace Avlu.AdminApp.Views;

public partial class PropertyListPage : ContentPage
{
    private readonly PropertyListViewModel _vm;

    public PropertyListPage(PropertyListViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
        BindingContext = _vm;
        PropertiesList.SetBinding(ItemsView.ItemsSourceProperty, nameof(_vm.Properties));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);

        if (_vm.ErrorMessage is not null)
            StateHost.CurrentState = DataStateView.ViewState.Error;
        else if (_vm.Properties.Count == 0)
            StateHost.CurrentState = DataStateView.ViewState.Empty;
        else
            StateHost.CurrentState = DataStateView.ViewState.Content;
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
        => _vm.SetSearch(e.NewTextValue ?? string.Empty);

    private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selected)
            _vm.SetFilter(selected);
    }

    private async void OnPropertyTapped(object sender, System.EventArgs e)
    {
        if (e is TappedEventArgs tapped && tapped.Parameter is PropertyDto p)
            await Shell.Current.GoToAsync($"property-detail?propertyId={p.Id}");
    }
}
