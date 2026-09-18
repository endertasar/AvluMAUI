using System.Text.Json.Serialization;

namespace AvluMAUI.Models;

public class ApiResponse<T>
{
    [JsonPropertyName("success")] public bool Success  { get; set; }
    [JsonPropertyName("data")]    public T?   Data     { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("errors")]  public List<string>? Errors { get; set; }
}
