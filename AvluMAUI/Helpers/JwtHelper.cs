using System.Text;
using System.Text.Json;

namespace AvluMAUI.Helpers;

public static class JwtHelper
{
    /// <summary>
    /// Decodes the payload of a JWT token (without verification).
    /// </summary>
    public static Dictionary<string, JsonElement>? DecodePayload(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1];
            // Fix base64url padding
            payload = payload.Replace('-', '+').Replace('_', '/');
            var mod = payload.Length % 4;
            if (mod == 2) payload += "==";
            else if (mod == 3) payload += "=";

            var bytes = Convert.FromBase64String(payload);
            var json  = Encoding.UTF8.GetString(bytes);

            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        }
        catch
        {
            return null;
        }
    }

    public static string? GetClaim(string token, string claimName)
    {
        var payload = DecodePayload(token);
        if (payload is null) return null;
        return payload.TryGetValue(claimName, out var val)
            ? val.ToString()
            : null;
    }

    public static bool IsExpired(string token)
    {
        var expStr = GetClaim(token, "exp");
        if (!long.TryParse(expStr, out var exp)) return true;
        var expiry = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
        return DateTime.UtcNow >= expiry;
    }

}
