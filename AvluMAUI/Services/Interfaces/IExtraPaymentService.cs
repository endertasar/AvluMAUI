using AvluMAUI.Models;
using AvluMAUI.Models.ExtraPayment;

namespace AvluMAUI.Services.Interfaces;

public interface IExtraPaymentService
{
    Task<ApiResponse<List<ExtraPaymentDto>>>    GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<ExtraPaymentDetailDto>>    GetByIdAsync(long id, CancellationToken ct = default);
    Task<ApiResponse<ExtraPaymentDto>>          CreateAsync(CreateExtraPaymentRequest request, CancellationToken ct = default);
}
