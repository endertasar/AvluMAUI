using System.Text.Json.Serialization;

namespace AvluMAUI.Models.Expense;

public class ExpenseDto
{
    [JsonPropertyName("id")]              public long     Id              { get; set; }
    [JsonPropertyName("category")]        public string?  Category        { get; set; }
    [JsonPropertyName("amount")]          public decimal  Amount          { get; set; }
    [JsonPropertyName("expenseDate")]     public DateTime ExpenseDate     { get; set; }
    [JsonPropertyName("description")]     public string?  Description     { get; set; }
    [JsonPropertyName("createdByUserId")] public long?    CreatedByUserId { get; set; }
    [JsonPropertyName("createdAt")]       public DateTime CreatedAt       { get; set; }

    public string CategoryLabel => string.IsNullOrEmpty(Category) ? "Diğer" : Category;
    public string DateLabel     => ExpenseDate.ToString("dd MMM yyyy");
}

public class CreateExpenseRequest
{
    [JsonPropertyName("category")]    public string?  Category    { get; set; }
    [JsonPropertyName("amount")]      public decimal  Amount      { get; set; }
    [JsonPropertyName("expenseDate")] public DateTime ExpenseDate { get; set; }
    [JsonPropertyName("description")] public string?  Description { get; set; }
}

public class UpdateExpenseRequest
{
    [JsonPropertyName("category")]    public string?  Category    { get; set; }
    [JsonPropertyName("amount")]      public decimal  Amount      { get; set; }
    [JsonPropertyName("expenseDate")] public DateTime ExpenseDate { get; set; }
    [JsonPropertyName("description")] public string?  Description { get; set; }
}
