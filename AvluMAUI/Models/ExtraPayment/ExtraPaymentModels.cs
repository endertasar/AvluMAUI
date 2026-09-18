using System.Text.Json.Serialization;

namespace AvluMAUI.Models.ExtraPayment;

public class ExtraPaymentDto
{
    [JsonPropertyName("id")]                   public long     Id                   { get; set; }
    [JsonPropertyName("name")]                 public string   Name                 { get; set; } = string.Empty;
    [JsonPropertyName("amountPerProperty")]    public decimal  AmountPerProperty    { get; set; }
    [JsonPropertyName("totalAmount")]          public decimal? TotalAmount          { get; set; }
    [JsonPropertyName("installmentCount")]     public int      InstallmentCount     { get; set; }
    [JsonPropertyName("generatedChargeCount")] public int      GeneratedChargeCount { get; set; }
    [JsonPropertyName("createdAt")]            public DateTime CreatedAt            { get; set; }

    public string InstallmentLabel =>
        InstallmentCount == 1 ? "Tek seferlik" : $"{InstallmentCount} taksit";
}

public class ExtraPaymentDetailDto
{
    [JsonPropertyName("id")]               public long     Id               { get; set; }
    [JsonPropertyName("name")]             public string   Name             { get; set; } = string.Empty;
    [JsonPropertyName("amountPerProperty")]public decimal  AmountPerProperty{ get; set; }
    [JsonPropertyName("totalAmount")]      public decimal? TotalAmount      { get; set; }
    [JsonPropertyName("installmentCount")] public int      InstallmentCount { get; set; }
    [JsonPropertyName("createdAt")]        public DateTime CreatedAt        { get; set; }
    [JsonPropertyName("installments")]     public List<InstallmentSummaryDto> Installments { get; set; } = [];
}

public class InstallmentSummaryDto
{
    [JsonPropertyName("installmentNo")] public int     InstallmentNo { get; set; }
    [JsonPropertyName("amount")]        public decimal Amount        { get; set; }
    [JsonPropertyName("paidAmount")]    public decimal PaidAmount    { get; set; }
    [JsonPropertyName("totalCharges")]  public int     TotalCharges  { get; set; }
    [JsonPropertyName("paidCharges")]   public int     PaidCharges   { get; set; }
}

public class CreateExtraPaymentRequest
{
    [JsonPropertyName("name")]              public string      Name              { get; set; } = string.Empty;
    [JsonPropertyName("amountPerProperty")] public decimal     AmountPerProperty { get; set; }
    [JsonPropertyName("installmentCount")]  public int         InstallmentCount  { get; set; } = 1;
    [JsonPropertyName("propertyScope")]     public string      PropertyScope     { get; set; } = "All";
    [JsonPropertyName("propertyIds")]       public List<long>? PropertyIds       { get; set; }
}
