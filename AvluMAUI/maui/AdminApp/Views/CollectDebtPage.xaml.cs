using Microsoft.Maui.Controls;
using Avlu.AdminApp.ViewModels;

namespace Avlu.AdminApp.Views;

public partial class CollectDebtPage : ContentPage
{
    private readonly CollectDebtViewModel _vm;

    public CollectDebtPage(CollectDebtViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
        BindingContext = _vm;
        ChargesList.SetBinding(ItemsView.ItemsSourceProperty, nameof(_vm.Charges));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);

        if (_vm.Property is not null)
        {
            PropertyTitleLabel.Text = _vm.Property.TitleLine;
            PropertyOwnerLabel.Text = _vm.Property.Owner;
        }
    }

    private async void OnChargeTapped(object sender, System.EventArgs e)
    {
        if (e is TappedEventArgs tapped && tapped.Parameter is ChargeItem charge)
        {
            var title = Uri.EscapeDataString(charge.Title);
            await Shell.Current.GoToAsync(
                $"collect-form?propertyId={_vm.PropertyId}&targetType={charge.TargetType}&targetId={charge.ChargeId}&amount={charge.Remaining}&title={title}");
        }
    }
}
