using System.Text.Json.Serialization;

namespace AvluMAUI.Models.Dues;

public class DuesDefinition
{
    [JsonPropertyName("id")]            public long     Id            { get; set; }
    [JsonPropertyName("siteId")]        public long     SiteId        { get; set; }
    [JsonPropertyName("propertyType")]  public string   PropertyType  { get; set; } = string.Empty;
    [JsonPropertyName("amount")]        public decimal  Amount        { get; set; }
    [JsonPropertyName("effectiveFrom")] public int      EffectiveFrom { get; set; }
    [JsonPropertyName("description")]   public string?  Description   { get; set; }
    [JsonPropertyName("createdAt")]     public DateTime CreatedAt     { get; set; }

    public string DisplayName =>
        $"{PropertyType} — {Amount:N2} ₺ ({EffectiveFrom / 100}/{EffectiveFrom % 100:D2}'den)";
}

public class CreateDuesDefinitionRequest
{
    [JsonPropertyName("propertyType")]  public string  PropertyType  { get; set; } = string.Empty;
    [JsonPropertyName("amount")]        public decimal Amount        { get; set; }
    [JsonPropertyName("effectiveFrom")] public int     EffectiveFrom { get; set; }
    [JsonPropertyName("description")]   public string? Description   { get; set; }
}

public class UpdateDuesDefinitionRequest
{
    [JsonPropertyName("propertyType")]  public string  PropertyType  { get; set; } = string.Empty;
    [JsonPropertyName("amount")]        public decimal Amount        { get; set; }
    [JsonPropertyName("effectiveFrom")] public int     EffectiveFrom { get; set; }
    [JsonPropertyName("description")]   public string? Description   { get; set; }
}

public class DuesChargeDto
{
    [JsonPropertyName("id")]              public long     Id              { get; set; }
    [JsonPropertyName("propertyId")]      public long     PropertyId      { get; set; }
    [JsonPropertyName("period")]          public int      Period          { get; set; }
    [JsonPropertyName("amount")]          public decimal  Amount          { get; set; }
    [JsonPropertyName("paidAmount")]      public decimal  PaidAmount      { get; set; }
    [JsonPropertyName("remainingAmount")] public decimal  RemainingAmount { get; set; }
    [JsonPropertyName("dueDate")]         public DateTime? DueDate        { get; set; }
    [JsonPropertyName("status")]          public string   Status          { get; set; } = string.Empty;
    [JsonPropertyName("createdAt")]       public DateTime CreatedAt       { get; set; }

    public string StatusLabel => Status switch
    {
        "Pending" => "Ödenmedi",
        "Partial" => "Kısmi",
        "Paid"    => "Ödendi",
        _         => Status
    };

    public string PeriodLabel =>
        $"{Period / 100}/{Period % 100:D2}";
}

public class CreateChargeRequest
{
    [JsonPropertyName("propertyId")] public long      PropertyId { get; set; }
    [JsonPropertyName("period")]     public int        Period     { get; set; }
    [JsonPropertyName("amount")]     public decimal    Amount     { get; set; }
    [JsonPropertyName("dueDate")]    public DateTime?  DueDate    { get; set; }
}

public class ChargeSummaryDto
{
    [JsonPropertyName("totalDebt")]      public decimal TotalDebt      { get; set; }
    [JsonPropertyName("totalCollected")] public decimal TotalCollected { get; set; }
    [JsonPropertyName("totalOverdue")]   public decimal TotalOverdue   { get; set; }
    [JsonPropertyName("overdueCount")]   public int     OverdueCount   { get; set; }
}

public class BulkChargeRequest
{
    [JsonPropertyName("year")]      public int  Year      { get; set; }
    [JsonPropertyName("month")]     public int  Month     { get; set; }
    [JsonPropertyName("duesDefId")] public long DuesDefId { get; set; }
}
