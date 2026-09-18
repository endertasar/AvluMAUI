using System.Text.Json.Serialization;

namespace AvluMAUI.Models.Resident;

public class ResidentDto
{
    [JsonPropertyName("id")]              public long    Id              { get; set; }
    [JsonPropertyName("phone")]           public string  Phone           { get; set; } = string.Empty;
    [JsonPropertyName("fullName")]        public string? FullName        { get; set; }
    [JsonPropertyName("isPhoneVerified")] public bool    IsPhoneVerified { get; set; }
    [JsonPropertyName("createdAt")]       public DateTime CreatedAt      { get; set; }
}

public class CreateResidentRequest
{
    [JsonPropertyName("phone")]    public string  Phone    { get; set; } = string.Empty;
    [JsonPropertyName("password")] public string  Password { get; set; } = string.Empty;
    [JsonPropertyName("fullName")] public string? FullName { get; set; }
}

public class UpdateResidentRequest
{
    [JsonPropertyName("fullName")]        public string? FullName        { get; set; }
    [JsonPropertyName("oneSignalUserId")] public string? OneSignalUserId { get; set; }
}
