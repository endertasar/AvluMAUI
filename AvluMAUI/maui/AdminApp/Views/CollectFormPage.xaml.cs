using Microsoft.Maui.Controls;
using Avlu.AdminApp.ViewModels;

namespace Avlu.AdminApp.Views;

public partial class CollectFormPage : ContentPage
{
    private readonly CollectFormViewModel _vm;

    public CollectFormPage(CollectFormViewModel vm)
    {
        _vm = vm;
        InitializeComponent();
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Show charge title in page header
        if (!string.IsNullOrEmpty(_vm.ChargeTitle))
            Title = _vm.ChargeTitle;

        // Pre-fill amount entry
        AmountEntry.Text = _vm.Amount > 0 ? $"{_vm.Amount:N2}" : string.Empty;
    }

    private async void OnSaveTapped(object sender, System.EventArgs e)
    {
        if (decimal.TryParse(AmountEntry.Text?.Replace(",", "."), out var parsed))
            _vm.Amount = parsed;

        _vm.Note = NoteEntry?.Text;

        if (_vm.ErrorMessage is not null)
        {
            await DisplayAlert("Hata", _vm.ErrorMessage, "Tamam");
            return;
        }

        await _vm.SaveCommand.ExecuteAsync(null);

        if (_vm.ErrorMessage is not null)
            await DisplayAlert("Hata", _vm.ErrorMessage, "Tamam");
    }
}
