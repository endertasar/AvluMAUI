using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Payment;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Payments;

[QueryProperty(nameof(PropertyId), "propertyId")]
public partial class PaymentListViewModel : ObservableObject
{
    private readonly IPaymentService _payments;

    [ObservableProperty] private long    _propertyId;
    [ObservableProperty] private bool    _isBusy = false;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<PaymentDto> Payments { get; } = [];

    public PaymentListViewModel(IPaymentService payments)
    {
        _payments = payments;
    }

    partial void OnPropertyIdChanged(long value) => _ = LoadAsync();

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (PropertyId == 0) return;
        IsBusy = true;
        ErrorMessage = null;
        Payments.Clear();
        try
        {
            var result = await _payments.GetByPropertyAsync(PropertyId);
            if (!result.Success || result.Data is null)
            {
                ErrorMessage = result.Message ?? "Ödemeler yüklenemedi.";
                return;
            }
            foreach (var p in result.Data.OrderByDescending(x => x.PaidAt)) Payments.Add(p);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task DeleteAsync(PaymentDto payment)
    {
        var confirmed = await AlertHelper.ShowConfirmAsync("Bu ödemeyi silmek istediğinizden emin misiniz?", "Ödemeyi Sil");
        if (!confirmed) return;
        IsBusy = true;
        try
        {
            var result = await _payments.DeleteAsync(payment.Id);
            if (!result.Success)
                await AlertHelper.ShowErrorAsync(result.Message ?? "Silme başarısız.");
            else
            {
                Payments.Remove(payment);
                await AlertHelper.ShowSuccessAsync("Ödeme silindi.");
            }
        }
        finally { IsBusy = false; }
    }
}
