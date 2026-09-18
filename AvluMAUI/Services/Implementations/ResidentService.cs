using AvluMAUI.Models;
using AvluMAUI.Models.Resident;
using AvluMAUI.Services.Base;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.Services.Implementations;

public class ResidentService : BaseApiService, IResidentService
{
    public ResidentService(HttpClient http) : base(http) { }

    public Task<ApiResponse<ResidentDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        GetAsync<ResidentDto>($"api/v1/residents/{id}", ct);

    public Task<ApiResponse<ResidentDto>> CreateAsync(CreateResidentRequest request, CancellationToken ct = default) =>
        PostAsync<ResidentDto>("api/v1/residents", request, ct);

    public Task<ApiResponse<bool>> UpdateAsync(long id, UpdateResidentRequest request, CancellationToken ct = default) =>
        PutAsync<bool>($"api/v1/residents/{id}", request, ct);

    public Task<ApiResponse<bool>> DeleteAsync(long id, CancellationToken ct = default) =>
        DeleteAsync<bool>($"api/v1/residents/{id}", ct);
}
