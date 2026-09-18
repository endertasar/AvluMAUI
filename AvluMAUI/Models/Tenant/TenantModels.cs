using System.Text.Json.Serialization;

namespace AvluMAUI.Models.Tenant;

public class TenantDto
{
    [JsonPropertyName("tenantId")]   public Guid   TenantId   { get; set; }
    [JsonPropertyName("tenantName")] public string TenantName { get; set; } = string.Empty;
    [JsonPropertyName("tenantCode")] public string TenantCode { get; set; } = string.Empty;
    [JsonPropertyName("address")]    public string? Address   { get; set; }
    [JsonPropertyName("isActive")]   public bool   IsActive   { get; set; }
    [JsonPropertyName("createdAt")]  public DateTime CreatedAt { get; set; }
}

public class AdminTenantDto
{
    [JsonPropertyName("adminId")]    public long    AdminId    { get; set; }
    [JsonPropertyName("username")]   public string  Username   { get; set; } = string.Empty;
    [JsonPropertyName("email")]      public string? Email      { get; set; }
    [JsonPropertyName("role")]       public string  Role       { get; set; } = string.Empty;
    [JsonPropertyName("assignedAt")] public DateTime AssignedAt { get; set; }
}

public class CreateTenantRequest
{
    [JsonPropertyName("tenantName")] public string TenantName { get; set; } = string.Empty;
    [JsonPropertyName("tenantCode")] public string TenantCode { get; set; } = string.Empty;
    [JsonPropertyName("address")]    public string? Address   { get; set; }
}

public class UpdateTenantRequest
{
    [JsonPropertyName("tenantName")] public string TenantName { get; set; } = string.Empty;
    [JsonPropertyName("address")]    public string? Address   { get; set; }
    [JsonPropertyName("isActive")]   public bool   IsActive   { get; set; } = true;
}

public class AssignAdminRequest
{
    [JsonPropertyName("adminId")] public long   AdminId { get; set; }
    [JsonPropertyName("role")]    public string Role    { get; set; } = "Manager";
}
