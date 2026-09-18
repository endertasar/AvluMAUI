using AvluMAUI.Models;
using AvluMAUI.Models.Payment;
using AvluMAUI.Services.Base;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.Services.Implementations;

public class PaymentService : BaseApiService, IPaymentService
{
    public PaymentService(HttpClient http) : base(http) { }

    public Task<ApiResponse<List<PaymentDto>>> GetByPropertyAsync(long propertyId, CancellationToken ct = default) =>
        GetAsync<List<PaymentDto>>($"api/v1/payments?propertyId={propertyId}", ct);

    public Task<ApiResponse<CreatePaymentResponse>> CreateAsync(CreatePaymentRequest request, CancellationToken ct = default) =>
        PostAsync<CreatePaymentResponse>("api/v1/payments", request, ct);

    public Task<ApiResponse<bool>> DeleteAsync(long id, CancellationToken ct = default) =>
        DeleteAsync<bool>($"api/v1/payments/{id}", ct);
}
