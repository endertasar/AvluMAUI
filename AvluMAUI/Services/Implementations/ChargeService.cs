using AvluMAUI.Models;
using AvluMAUI.Models.Dues;
using AvluMAUI.Services.Base;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.Services.Implementations;

public class ChargeService : BaseApiService, IChargeService
{
    public ChargeService(HttpClient http) : base(http) { }

    public Task<ApiResponse<List<DuesChargeDto>>> GetByPropertyAsync(long propertyId, CancellationToken ct = default) =>
        GetAsync<List<DuesChargeDto>>($"api/v1/charges?propertyId={propertyId}", ct);

    public Task<ApiResponse<ChargeSummaryDto>> GetSummaryAsync(CancellationToken ct = default) =>
        GetAsync<ChargeSummaryDto>("api/v1/charges/summary", ct);

    public Task<ApiResponse<DuesChargeDto>> SingleChargeAsync(CreateChargeRequest request, CancellationToken ct = default) =>
        PostAsync<DuesChargeDto>("api/v1/dues/charges", request, ct);
}
