using AvluMAUI.Models;
using AvluMAUI.Models.Dues;
using AvluMAUI.Services.Base;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.Services.Implementations;

public class DuesService : BaseApiService, IDuesService
{
    public DuesService(HttpClient http) : base(http) { }

    public Task<ApiResponse<List<DuesDefinition>>> GetDefinitionsAsync(CancellationToken ct = default) =>
        GetAsync<List<DuesDefinition>>("api/v1/dues/definitions", ct);

    public Task<ApiResponse<DuesDefinition>> CreateDefinitionAsync(CreateDuesDefinitionRequest request, CancellationToken ct = default) =>
        PostAsync<DuesDefinition>("api/v1/dues/definitions", request, ct);

    public Task<ApiResponse<bool>> DeleteDefinitionAsync(long id, CancellationToken ct = default) =>
        DeleteAsync<bool>($"api/v1/dues/definitions/{id}", ct);
}
