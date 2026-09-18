using Microsoft.Maui.Controls;
using Avlu.AdminApp.ViewModels;

namespace Avlu.AdminApp.Views;

public partial class CollectPropertyPage : ContentPage
{
    private readonly CollectPropertyViewModel _vm;

    public CollectPropertyPage(CollectPropertyViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
        DebtorsList.SetBinding(ItemsView.ItemsSourceProperty, nameof(_vm.Debtors));
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }

    private async void OnPropertyTapped(object sender, System.EventArgs e)
    {
        if (e is TappedEventArgs tapped && tapped.Parameter is DebtorItem item)
            await Shell.Current.GoToAsync($"collect-debt?propertyId={item.PropertyId}");
    }
}
