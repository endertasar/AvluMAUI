using AvluMAUI.Models;
using AvluMAUI.Models.Payment;

namespace AvluMAUI.Services.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<List<PaymentDto>>>      GetByPropertyAsync(long propertyId, CancellationToken ct = default);
    Task<ApiResponse<CreatePaymentResponse>> CreateAsync(CreatePaymentRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>>                  DeleteAsync(long id, CancellationToken ct = default);
}
