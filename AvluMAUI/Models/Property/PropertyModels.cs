using System.Text.Json.Serialization;

namespace AvluMAUI.Models.Property;

public class PropertyDto
{
    [JsonPropertyName("id")]              public long     Id              { get; set; }
    [JsonPropertyName("block")]           public string?  Block           { get; set; }
    [JsonPropertyName("unitNo")]          public string   UnitNo          { get; set; } = string.Empty;
    [JsonPropertyName("type")]            public string   Type            { get; set; } = string.Empty;
    [JsonPropertyName("ownerName")]       public string?  OwnerName       { get; set; }
    [JsonPropertyName("ownerPhone")]      public string?  OwnerPhone      { get; set; }
    [JsonPropertyName("residentName")]    public string?  ResidentName    { get; set; }
    [JsonPropertyName("residentPhone")]   public string?  ResidentPhone   { get; set; }
    [JsonPropertyName("duesResponsible")] public string   DuesResponsible { get; set; } = string.Empty;
    [JsonPropertyName("isActive")]        public bool     IsActive        { get; set; }
    [JsonPropertyName("createdAt")]       public DateTime CreatedAt       { get; set; }

    public string DisplayName =>
        string.IsNullOrEmpty(Block) ? UnitNo : $"{Block} – {UnitNo}";

    public string TypeLabel => Type switch
    {
        "Daire"   => "Daire",
        "Dukkan"  => "Dükkan",
        "Otopark" => "Otopark",
        _         => Type
    };

    public string BlockUnit => Block?.Length > 0 ? $"{Block[0]}{UnitNo}" : $"?{UnitNo}";
    public string TitleLine => string.IsNullOrEmpty(Block) ? UnitNo : $"{Block} · {UnitNo}";
    public string Owner     => OwnerName ?? ResidentName ?? "—";
    public string DebtDisplay => "—";
}

public class CreatePropertyRequest
{
    [JsonPropertyName("block")]           public string?  Block           { get; set; }
    [JsonPropertyName("unitNo")]          public string   UnitNo          { get; set; } = string.Empty;
    [JsonPropertyName("type")]            public string   Type            { get; set; } = "Daire";
    [JsonPropertyName("ownerName")]       public string?  OwnerName       { get; set; }
    [JsonPropertyName("ownerPhone")]      public string?  OwnerPhone      { get; set; }
    [JsonPropertyName("residentName")]    public string?  ResidentName    { get; set; }
    [JsonPropertyName("residentPhone")]   public string?  ResidentPhone   { get; set; }
    [JsonPropertyName("duesResponsible")] public string?  DuesResponsible { get; set; }
}

public class UpdatePropertyRequest
{
    [JsonPropertyName("block")]           public string?  Block           { get; set; }
    [JsonPropertyName("unitNo")]          public string   UnitNo          { get; set; } = string.Empty;
    [JsonPropertyName("type")]            public string   Type            { get; set; } = "Daire";
    [JsonPropertyName("ownerName")]       public string?  OwnerName       { get; set; }
    [JsonPropertyName("ownerPhone")]      public string?  OwnerPhone      { get; set; }
    [JsonPropertyName("residentName")]    public string?  ResidentName    { get; set; }
    [JsonPropertyName("residentPhone")]   public string?  ResidentPhone   { get; set; }
    [JsonPropertyName("duesResponsible")] public string?  DuesResponsible { get; set; }
    [JsonPropertyName("isActive")]        public bool     IsActive        { get; set; } = true;
}
