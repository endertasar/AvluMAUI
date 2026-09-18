using Microsoft.Maui.Controls;
using Avlu.AdminApp.ViewModels;

namespace Avlu.AdminApp.Views;

public partial class PropertyDetailPage : ContentPage
{
    private readonly PropertyDetailViewModel _vm;

    public PropertyDetailPage(PropertyDetailViewModel vm)
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
        UpdateUI();
    }

    private void UpdateUI()
    {
        var p = _vm.Property;
        if (p is null) return;

        PropertyTitleLabel.Text  = p.TitleLine;
        PropertyTypeBadge.Text   = p.TypeLabel;
        TotalDebtLabel.Text      = $"{_vm.TotalDebt:N2} ₺";
        OwnerNameLabel.Text      = p.OwnerName ?? "—";
        OwnerPhoneLabel.Text     = p.OwnerPhone ?? "—";
        DuesResponsibleLabel.Text = p.DuesResponsible switch
        {
            "Owner"    => "Sahip",
            "Resident" => "Kiracı",
            _          => p.DuesResponsible
        };
    }

    private async void OnCollectTapped(object sender, System.EventArgs e)
        => await Shell.Current.GoToAsync($"collect-debt?propertyId={_vm.PropertyId}");
}
