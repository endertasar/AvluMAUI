using AvluMAUI.Models;
using AvluMAUI.Models.Auth;
using AvluMAUI.Services.Base;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.Services.Implementations;

public class AuthService : BaseApiService, IAuthService
{
    public AuthService(HttpClient http) : base(http) { }

    public Task<ApiResponse<AdminLoginResponse>> LoginAdminAsync(AdminLoginRequest request, CancellationToken ct = default) =>
        PostAsync<AdminLoginResponse>("api/v1/auth/admin/login", request, ct);

    public Task<ApiResponse<AdminRegisterResponse>> RegisterAdminAsync(AdminRegisterRequest request, CancellationToken ct = default) =>
        PostAsync<AdminRegisterResponse>("api/v1/auth/admin/register", request, ct);

    public Task<ApiResponse<TokenRefreshResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default) =>
        PostAsync<TokenRefreshResponse>("api/v1/auth/refresh", request, ct);

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default) =>
        await PostAsync<object>("api/v1/auth/logout", new LogoutRequest { RefreshToken = refreshToken }, ct);
}
