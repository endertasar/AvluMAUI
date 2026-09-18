using AvluMAUI.Models;
using AvluMAUI.Models.Property;
using AvluMAUI.Services.Base;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.Services.Implementations;

public class PropertyService : BaseApiService, IPropertyService
{
    public PropertyService(HttpClient http) : base(http) { }

    public Task<ApiResponse<List<PropertyDto>>> GetAllAsync(CancellationToken ct = default) =>
        GetAsync<List<PropertyDto>>("api/v1/properties", ct);

    public Task<ApiResponse<PropertyDto>> GetByIdAsync(long propertyId, CancellationToken ct = default) =>
        GetAsync<PropertyDto>($"api/v1/properties/{propertyId}", ct);

    public Task<ApiResponse<PropertyDto>> CreateAsync(CreatePropertyRequest request, CancellationToken ct = default) =>
        PostAsync<PropertyDto>("api/v1/properties", request, ct);

    public Task<ApiResponse<PropertyDto>> UpdateAsync(long propertyId, UpdatePropertyRequest request, CancellationToken ct = default) =>
        PutAsync<PropertyDto>($"api/v1/properties/{propertyId}", request, ct);

    public Task<ApiResponse<bool>> DeleteAsync(long propertyId, CancellationToken ct = default) =>
        DeleteAsync<bool>($"api/v1/properties/{propertyId}", ct);
}
