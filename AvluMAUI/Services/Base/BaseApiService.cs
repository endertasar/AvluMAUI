using System.Net.Http.Json;
using System.Text.Json;
using AvluMAUI.Models;
using AvluMAUI.Helpers;

namespace AvluMAUI.Services.Base;

public abstract class BaseApiService
{
    protected readonly HttpClient Http;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected BaseApiService(HttpClient http)
    {
        Http = http;
    }

    protected async Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken ct = default)
    {
        try
        {
            var response = await Http.GetAsync(url, ct);
            return await ReadResponseAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return Fail<T>(ex.Message);
        }
    }

    protected async Task<ApiResponse<T>> PostAsync<T>(string url, object? body = null, CancellationToken ct = default)
    {
        var requestJson = body is null ? "(null)" : JsonSerializer.Serialize(body, JsonOpts);
        System.Diagnostics.Debug.WriteLine($"[HTTP POST] {Http.BaseAddress}{url}");
        System.Diagnostics.Debug.WriteLine($"[HTTP POST] Body: {requestJson}");
        try
        {
            var response = await Http.PostAsJsonAsync(url, body, JsonOpts, ct);
            System.Diagnostics.Debug.WriteLine($"[HTTP POST] Status: {(int)response.StatusCode} {response.StatusCode}");
            var result = await ReadResponseAsync<T>(response, ct);
            System.Diagnostics.Debug.WriteLine($"[HTTP POST] Parsed: Success={result.Success} Message={result.Message}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HTTP POST] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
            return Fail<T>(ex.Message);
        }
    }

    protected async Task<ApiResponse<T>> PutAsync<T>(string url, object? body = null, CancellationToken ct = default)
    {
        try
        {
            var response = await Http.PutAsJsonAsync(url, body, JsonOpts, ct);
            return await ReadResponseAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return Fail<T>(ex.Message);
        }
    }

    protected async Task<ApiResponse<T>> DeleteAsync<T>(string url, CancellationToken ct = default)
    {
        try
        {
            var response = await Http.DeleteAsync(url, ct);
            return await ReadResponseAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return Fail<T>(ex.Message);
        }
    }

    private static async Task<ApiResponse<T>> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var json = await response.Content.ReadAsStringAsync(ct);

        // 401 Unauthorized → clear session
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return new ApiResponse<T> { Success = false, Message = "Oturum süresi doldu." };
        }

        if (string.IsNullOrWhiteSpace(json))
            return new ApiResponse<T> { Success = false, Message = "Sunucudan yanıt alınamadı." };

        try
        {
            var result = JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOpts);
            return result ?? Fail<T>("Yanıt ayrıştırılamadı.");
        }
        catch
        {
            return Fail<T>($"Yanıt hatalı: {json[..Math.Min(json.Length, 200)]}");
        }
    }

    private static ApiResponse<T> Fail<T>(string message) =>
        new() { Success = false, Message = message };
}
