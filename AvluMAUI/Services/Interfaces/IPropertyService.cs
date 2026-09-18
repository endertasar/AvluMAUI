using AvluMAUI.Models;
using AvluMAUI.Models.Property;

namespace AvluMAUI.Services.Interfaces;

public interface IPropertyService
{
    Task<ApiResponse<List<PropertyDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<PropertyDto>>       GetByIdAsync(long propertyId, CancellationToken ct = default);
    Task<ApiResponse<PropertyDto>>       CreateAsync(CreatePropertyRequest request, CancellationToken ct = default);
    Task<ApiResponse<PropertyDto>>       UpdateAsync(long propertyId, UpdatePropertyRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>>              DeleteAsync(long propertyId, CancellationToken ct = default);
}
