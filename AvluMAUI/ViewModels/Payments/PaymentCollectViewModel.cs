using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AvluMAUI.Helpers;
using AvluMAUI.Models.Dues;
using AvluMAUI.Models.Payment;
using AvluMAUI.Models.Property;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.ViewModels.Payments;

public partial class PaymentCollectViewModel : ObservableObject
{
    private readonly IPaymentService  _payments;
    private readonly IPropertyService _properties;
    private readonly IChargeService   _charges;

    [ObservableProperty] private bool    _isBusy  = false;
    [ObservableProperty] private string  _amount  = string.Empty;
    [ObservableProperty] private string  _method  = "Cash";
    [ObservableProperty] private string? _note;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<PropertyDto>   Properties { get; } = [];
    public ObservableCollection<DuesChargeDto> Charges    { get; } = [];
    public List<string> PaymentMethods { get; } = ["Cash", "Transfer", "Card"];

    private PropertyDto?   _selectedProperty;
    private DuesChargeDto? _selectedCharge;

    public PropertyDto? SelectedProperty
    {
        get => _selectedProperty;
        set { if (SetProperty(ref _selectedProperty, value) && value is not null) _ = LoadChargesAsync(value.Id); }
    }
    public DuesChargeDto? SelectedCharge
    {
        get => _selectedCharge;
        set { SetProperty(ref _selectedCharge, value); if (value is not null) Amount = value.RemainingAmount.ToString(); }
    }

    public PaymentCollectViewModel(IPaymentService payments, IPropertyService properties, IChargeService charges)
    {
        _payments   = payments;
        _properties = properties;
        _charges    = charges;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        Properties.Clear();
        try
        {
            var result = await _properties.GetAllAsync();
            if (result.Success && result.Data is not null)
                foreach (var p in result.Data) Properties.Add(p);
        }
        finally { IsBusy = false; }
    }

    private async Task LoadChargesAsync(long propertyId)
    {
        Charges.Clear();
        var result = await _charges.GetByPropertyAsync(propertyId);
        if (result.Success && result.Data is not null)
            foreach (var c in result.Data.Where(c => c.Status != "Paid")) Charges.Add(c);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedCharge is null || !decimal.TryParse(Amount, out var amount) || amount <= 0)
        {
            ErrorMessage = "Borç ve geçerli tutar zorunludur.";
            return;
        }
        IsBusy       = true;
        ErrorMessage = null;
        try
        {
            var req = new CreatePaymentRequest
            {
                ChargeId = SelectedCharge.Id,
                Amount   = amount,
                Method   = Method,
                Note     = string.IsNullOrWhiteSpace(Note) ? null : Note
            };
            var result = await _payments.CreateAsync(req);
            if (!result.Success) { ErrorMessage = result.Message; return; }
            await AlertHelper.ShowSuccessAsync("Ödeme kaydedildi.");
            if (SelectedProperty is not null)
                await LoadChargesAsync(SelectedProperty.Id);
            Amount         = string.Empty;
            SelectedCharge = null;
        }
        finally { IsBusy = false; }
    }
}
