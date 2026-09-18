using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvluMAUI.Models.Payment;
using AvluMAUI.Services.Interfaces;

namespace Avlu.AdminApp.ViewModels;

[QueryProperty(nameof(PropertyId), "propertyId")]
[QueryProperty(nameof(TargetType), "targetType")]
[QueryProperty(nameof(TargetId),   "targetId")]
[QueryProperty(nameof(Amount),     "amount")]
[QueryProperty(nameof(ChargeTitle), "title")]
public partial class CollectFormViewModel : ObservableObject
{
    private readonly IPaymentService _paymentService;

    [ObservableProperty] private long    _propertyId;
    [ObservableProperty] private string  _targetType  = "Dues";
    [ObservableProperty] private long    _targetId;
    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private string  _chargeTitle = string.Empty;
    [ObservableProperty] private string  _method      = "Transfer";
    [ObservableProperty] private string? _note;
    [ObservableProperty] private bool    _useCredit;
    [ObservableProperty] private bool    _isLoading;
    [ObservableProperty] private string? _errorMessage;

    public string[] Methods { get; } = ["Nakit", "Havale", "Kart"];

    public CollectFormViewModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [RelayCommand]
    public async Task SaveAsync(CancellationToken ct = default)
    {
        if (Amount <= 0)
        {
            ErrorMessage = "Tutar sıfırdan büyük olmalı.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var methodCode = Method switch
            {
                "Nakit"  => "Cash",
                "Havale" => "Transfer",
                "Kart"   => "Card",
                _        => Method
            };

            var request = new CreatePaymentRequest
            {
                PropertyId = PropertyId,
                TargetType = TargetType,
                TargetId   = TargetId,
                Amount     = Amount,
                Method     = methodCode,
                Note       = string.IsNullOrWhiteSpace(Note) ? null : Note,
                UseCredit  = UseCredit ? true : null
            };

            var result = await _paymentService.CreateAsync(request, ct);
            if (result.Success)
            {
                await Shell.Current.GoToAsync("collect-success");
            }
            else
            {
                ErrorMessage = result.Message ?? "Tahsilat kaydedilemedi.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
