using AvluMAUI.Models;
using AvluMAUI.Models.Dues;

namespace AvluMAUI.Services.Interfaces;

public interface IDuesService
{
    Task<ApiResponse<List<DuesDefinition>>> GetDefinitionsAsync(CancellationToken ct = default);
    Task<ApiResponse<DuesDefinition>>       CreateDefinitionAsync(CreateDuesDefinitionRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>>                 DeleteDefinitionAsync(long id, CancellationToken ct = default);
}

public interface IChargeService
{
    Task<ApiResponse<List<DuesChargeDto>>> GetByPropertyAsync(long propertyId, CancellationToken ct = default);
    Task<ApiResponse<ChargeSummaryDto>>    GetSummaryAsync(CancellationToken ct = default);
    Task<ApiResponse<DuesChargeDto>>       SingleChargeAsync(CreateChargeRequest request, CancellationToken ct = default);
}
