using Microsoft.Maui.Controls;
namespace Avlu.AdminApp.Views;
public partial class ExtraPaymentFormPage : ContentPage
{
    private int _installments = 6;
    public ExtraPaymentFormPage() { InitializeComponent(); }
    private void OnIncrement(object sender, System.EventArgs e) { _installments++; InstallmentValue.Text = _installments.ToString(); }
    private void OnDecrement(object sender, System.EventArgs e) { if (_installments > 1) _installments--; InstallmentValue.Text = _installments.ToString(); }
    private async void OnSaveTapped(object sender, System.EventArgs e) => await Shell.Current.GoToAsync("..");
}
