using System.Text.Json.Serialization;

namespace AvluMAUI.Models.Auth;

public class AdminLoginRequest
{
    [JsonPropertyName("siteUsername")] public string SiteUsername { get; set; } = string.Empty;
    [JsonPropertyName("username")]     public string Username     { get; set; } = string.Empty;
    [JsonPropertyName("password")]     public string Password     { get; set; } = string.Empty;
}

public class AdminRegisterRequest
{
    [JsonPropertyName("siteUsername")] public string  SiteUsername { get; set; } = string.Empty;
    [JsonPropertyName("siteName")]     public string  SiteName     { get; set; } = string.Empty;
    [JsonPropertyName("address")]      public string? Address      { get; set; }
    [JsonPropertyName("username")]     public string  Username     { get; set; } = string.Empty;
    [JsonPropertyName("password")]     public string  Password     { get; set; } = string.Empty;
    [JsonPropertyName("fullName")]     public string? FullName     { get; set; }
}

public class RefreshTokenRequest
{
    [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequest
{
    [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = string.Empty;
}

public class AdminLoginResponse
{
    [JsonPropertyName("accessToken")]  public string        AccessToken  { get; set; } = string.Empty;
    [JsonPropertyName("refreshToken")] public string        RefreshToken { get; set; } = string.Empty;
    [JsonPropertyName("user")]         public AdminUserInfo? User        { get; set; }
}

public class AdminRegisterResponse
{
    [JsonPropertyName("accessToken")]  public string   AccessToken  { get; set; } = string.Empty;
    [JsonPropertyName("refreshToken")] public string   RefreshToken { get; set; } = string.Empty;
    [JsonPropertyName("site")]         public SiteInfo? Site        { get; set; }
}

public class TokenRefreshResponse
{
    [JsonPropertyName("accessToken")]  public string AccessToken  { get; set; } = string.Empty;
    [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = string.Empty;
}

public class AdminUserInfo
{
    [JsonPropertyName("id")]       public long    Id       { get; set; }
    [JsonPropertyName("username")] public string  Username { get; set; } = string.Empty;
    [JsonPropertyName("fullName")] public string? FullName { get; set; }
    [JsonPropertyName("role")]     public string  Role     { get; set; } = string.Empty;
    [JsonPropertyName("isActive")] public bool    IsActive { get; set; }
}

public class SiteInfo
{
    [JsonPropertyName("id")]           public long   Id           { get; set; }
    [JsonPropertyName("siteUsername")] public string SiteUsername { get; set; } = string.Empty;
    [JsonPropertyName("name")]         public string Name         { get; set; } = string.Empty;
}
