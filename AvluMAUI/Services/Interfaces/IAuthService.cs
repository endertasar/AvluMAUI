using AvluMAUI.Models;
using AvluMAUI.Models.Auth;

namespace AvluMAUI.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AdminLoginResponse>>    LoginAdminAsync(AdminLoginRequest request, CancellationToken ct = default);
    Task<ApiResponse<AdminRegisterResponse>> RegisterAdminAsync(AdminRegisterRequest request, CancellationToken ct = default);
    Task<ApiResponse<TokenRefreshResponse>>  RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
}
