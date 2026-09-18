using System.Text.Json.Serialization;

namespace AvluMAUI.Models.Payment;

public class PaymentDto
{
    [JsonPropertyName("id")]                public long     Id                { get; set; }
    [JsonPropertyName("propertyId")]        public long     PropertyId        { get; set; }
    [JsonPropertyName("targetType")]        public string   TargetType        { get; set; } = string.Empty;
    [JsonPropertyName("targetId")]          public long     TargetId          { get; set; }
    [JsonPropertyName("amount")]            public decimal  Amount            { get; set; }
    [JsonPropertyName("method")]            public string   Method            { get; set; } = string.Empty;
    [JsonPropertyName("note")]              public string?  Note              { get; set; }
    [JsonPropertyName("paidAt")]            public DateTime PaidAt            { get; set; }
    [JsonPropertyName("collectedByUserId")] public long?    CollectedByUserId { get; set; }

    public string MethodLabel => Method switch
    {
        "Cash"     => "Nakit",
        "Transfer" => "Havale",
        "Card"     => "Kart",
        _          => Method
    };
}

public class CreatePaymentRequest
{
    [JsonPropertyName("propertyId")] public long    PropertyId  { get; set; }
    [JsonPropertyName("targetType")] public string  TargetType  { get; set; } = "Dues";
    [JsonPropertyName("targetId")]   public long    TargetId    { get; set; }
    [JsonPropertyName("amount")]     public decimal Amount      { get; set; }
    [JsonPropertyName("method")]     public string? Method      { get; set; }
    [JsonPropertyName("note")]       public string? Note        { get; set; }
    [JsonPropertyName("useCredit")]  public bool?   UseCredit   { get; set; }
}

public class CreatePaymentResponse
{
    [JsonPropertyName("id")]              public long     Id              { get; set; }
    [JsonPropertyName("propertyId")]      public long     PropertyId      { get; set; }
    [JsonPropertyName("targetType")]      public string   TargetType      { get; set; } = string.Empty;
    [JsonPropertyName("targetId")]        public long     TargetId        { get; set; }
    [JsonPropertyName("amount")]          public decimal  Amount          { get; set; }
    [JsonPropertyName("method")]          public string   Method          { get; set; } = string.Empty;
    [JsonPropertyName("paidAt")]          public DateTime PaidAt          { get; set; }
    [JsonPropertyName("chargeStatus")]    public string   ChargeStatus    { get; set; } = string.Empty;
    [JsonPropertyName("chargeRemaining")] public decimal  ChargeRemaining { get; set; }
    [JsonPropertyName("appliedToCredit")] public decimal  AppliedToCredit { get; set; }
    [JsonPropertyName("creditBalance")]   public decimal  CreditBalance   { get; set; }
}
