using AvluMAUI.Models;
using AvluMAUI.Models.Resident;

namespace AvluMAUI.Services.Interfaces;

public interface IResidentService
{
    Task<ApiResponse<ResidentDto>>       GetByIdAsync(long id, CancellationToken ct = default);
    Task<ApiResponse<ResidentDto>>       CreateAsync(CreateResidentRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>>              UpdateAsync(long id, UpdateResidentRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>>              DeleteAsync(long id, CancellationToken ct = default);
}
