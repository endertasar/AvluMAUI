using System.Text.Json.Serialization;

namespace AvluMAUI.Models.Report;

public class DashboardDto
{
    [JsonPropertyName("totalActiveProperties")]  public int     TotalActiveProperties  { get; set; }
    [JsonPropertyName("totalPendingDues")]       public decimal TotalPendingDues       { get; set; }
    [JsonPropertyName("currentMonthCharged")]    public decimal CurrentMonthCharged    { get; set; }
    [JsonPropertyName("currentMonthCollected")]  public decimal CurrentMonthCollected  { get; set; }
    [JsonPropertyName("overdueCount")]           public int     OverdueCount           { get; set; }
    [JsonPropertyName("totalExpensesYtd")]       public decimal TotalExpensesYtd       { get; set; }
    [JsonPropertyName("totalCreditBalance")]     public decimal TotalCreditBalance     { get; set; }

    public double CollectionRate =>
        CurrentMonthCharged > 0 ? (double)(CurrentMonthCollected / CurrentMonthCharged) : 0;

    public string CollectionRateLabel => $"%{CollectionRate * 100:F0}";
}

public class MonthlyCollectionDto
{
    [JsonPropertyName("year")]            public int     Year            { get; set; }
    [JsonPropertyName("month")]           public int     Month           { get; set; }
    [JsonPropertyName("totalCharged")]    public decimal TotalCharged    { get; set; }
    [JsonPropertyName("totalCollected")]  public decimal TotalCollected  { get; set; }
    [JsonPropertyName("totalPending")]    public decimal TotalPending    { get; set; }
    [JsonPropertyName("totalProperties")] public int     TotalProperties { get; set; }

    public double CollectionRate =>
        TotalCharged > 0 ? (double)(TotalCollected / TotalCharged) : 0;
}

public class IncomeExpenseMonthDto
{
    [JsonPropertyName("year")]        public int     Year        { get; set; }
    [JsonPropertyName("month")]       public int     Month       { get; set; }
    [JsonPropertyName("incomeDues")]  public decimal IncomeDues  { get; set; }
    [JsonPropertyName("incomeExtra")] public decimal IncomeExtra { get; set; }
    [JsonPropertyName("expense")]     public decimal Expense     { get; set; }
    [JsonPropertyName("net")]         public decimal Net         { get; set; }
}

public class DebtAgingDto
{
    [JsonPropertyName("days0To30")]  public decimal Days0To30  { get; set; }
    [JsonPropertyName("days31To60")] public decimal Days31To60 { get; set; }
    [JsonPropertyName("days61To90")] public decimal Days61To90 { get; set; }
    [JsonPropertyName("daysOver90")] public decimal DaysOver90 { get; set; }

    public decimal Total => Days0To30 + Days31To60 + Days61To90 + DaysOver90;
}

public class PropertySummaryDto
{
    [JsonPropertyName("propertyId")] public long    PropertyId { get; set; }
    [JsonPropertyName("block")]      public string? Block      { get; set; }
    [JsonPropertyName("unitNo")]     public string  UnitNo     { get; set; } = string.Empty;
    [JsonPropertyName("totalDebt")]  public decimal TotalDebt  { get; set; }
    [JsonPropertyName("totalPaid")]  public decimal TotalPaid  { get; set; }
    [JsonPropertyName("balance")]    public decimal Balance    { get; set; }

    public string DisplayName =>
        string.IsNullOrEmpty(Block) ? UnitNo : $"{Block} – {UnitNo}";
}

public class OverdueChargeDto
{
    [JsonPropertyName("chargeId")]    public long      ChargeId    { get; set; }
    [JsonPropertyName("propertyId")]  public long      PropertyId  { get; set; }
    [JsonPropertyName("block")]       public string?   Block       { get; set; }
    [JsonPropertyName("unitNo")]      public string    UnitNo      { get; set; } = string.Empty;
    [JsonPropertyName("amount")]      public decimal   Amount      { get; set; }
    [JsonPropertyName("paidAmount")]  public decimal   PaidAmount  { get; set; }
    [JsonPropertyName("dueDate")]     public DateTime? DueDate     { get; set; }
    [JsonPropertyName("daysOverdue")] public int       DaysOverdue { get; set; }
    [JsonPropertyName("status")]      public string    Status      { get; set; } = string.Empty;

    public string DisplayName =>
        string.IsNullOrEmpty(Block) ? UnitNo : $"{Block} – {UnitNo}";
    public decimal RemainingAmount => Amount - PaidAmount;
}
