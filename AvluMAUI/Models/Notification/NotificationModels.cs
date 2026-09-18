using System.Text.Json.Serialization;

namespace AvluMAUI.Models.Notification;

public class NotificationDto
{
    [JsonPropertyName("notifId")]    public long     NotifId    { get; set; }
    [JsonPropertyName("title")]      public string   Title      { get; set; } = string.Empty;
    [JsonPropertyName("body")]       public string   Body       { get; set; } = string.Empty;
    [JsonPropertyName("targetType")] public string   TargetType { get; set; } = string.Empty;
    [JsonPropertyName("targetId")]   public string?  TargetId   { get; set; }
    [JsonPropertyName("sentAt")]     public DateTime SentAt     { get; set; }
    [JsonPropertyName("sentBy")]     public long?    SentBy     { get; set; }

    public string TargetLabel => TargetType switch
    {
        "All"      => "Tüm Sakinler",
        "Block"    => $"Blok: {TargetId}",
        "Property" => $"Daire: {TargetId}",
        "Resident" => $"Sakin: {TargetId}",
        _          => TargetType
    };
}

public class SendNotificationRequest
{
    [JsonPropertyName("title")]      public string  Title      { get; set; } = string.Empty;
    [JsonPropertyName("body")]       public string  Body       { get; set; } = string.Empty;
    [JsonPropertyName("targetType")] public string  TargetType { get; set; } = "All";
    [JsonPropertyName("targetId")]   public string? TargetId   { get; set; }
}
