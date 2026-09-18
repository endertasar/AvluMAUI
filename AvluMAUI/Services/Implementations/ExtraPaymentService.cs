using AvluMAUI.Models;
using AvluMAUI.Models.ExtraPayment;
using AvluMAUI.Services.Base;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.Services.Implementations;

public class ExtraPaymentService : BaseApiService, IExtraPaymentService
{
    public ExtraPaymentService(HttpClient http) : base(http) { }

    public Task<ApiResponse<List<ExtraPaymentDto>>> GetAllAsync(CancellationToken ct = default) =>
        GetAsync<List<ExtraPaymentDto>>("api/v1/extra-payments", ct);

    public Task<ApiResponse<ExtraPaymentDetailDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        GetAsync<ExtraPaymentDetailDto>($"api/v1/extra-payments/{id}", ct);

    public Task<ApiResponse<ExtraPaymentDto>> CreateAsync(CreateExtraPaymentRequest request, CancellationToken ct = default) =>
        PostAsync<ExtraPaymentDto>("api/v1/extra-payments", request, ct);
}
