namespace AvluMAUI.Helpers;

public class TokenManager
{
    private readonly HttpClient _http;
    private string? _accessToken;
    private string? _refreshToken;

    public string? AccessToken  => _accessToken;
    public string? RefreshToken => _refreshToken;
    public bool IsLoggedIn => !string.IsNullOrEmpty(_accessToken) && !JwtHelper.IsExpired(_accessToken);

    public TokenManager(HttpClient http) { _http = http; }

    public async Task LoadSavedTokenAsync()
    {
        try
        {
            _accessToken  = await SecureStorage.GetAsync(Constants.AccessTokenKey);
            _refreshToken = await SecureStorage.GetAsync(Constants.RefreshTokenKey);
        }
        catch
        {
            _accessToken  = null;
            _refreshToken = null;
        }

        if (!string.IsNullOrEmpty(_accessToken))
            ApplyToken(_accessToken);
    }

    public async Task SetTokensAsync(string accessToken, string refreshToken)
    {
        _accessToken  = accessToken;
        _refreshToken = refreshToken;
        await SecureStorage.SetAsync(Constants.AccessTokenKey, accessToken);
        await SecureStorage.SetAsync(Constants.RefreshTokenKey, refreshToken);
        ApplyToken(accessToken);
    }

    public async Task ClearAsync()
    {
        _accessToken  = null;
        _refreshToken = null;
        SecureStorage.Remove(Constants.AccessTokenKey);
        SecureStorage.Remove(Constants.RefreshTokenKey);
        _http.DefaultRequestHeaders.Authorization = null;
        await NavigationHelper.GoToLoginAsync();
    }

    private void ApplyToken(string token) =>
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
}
