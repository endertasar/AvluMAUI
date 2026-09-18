using AvluMAUI.Models;
using AvluMAUI.Models.Expense;
using AvluMAUI.Services.Base;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.Services.Implementations;

public class ExpenseService : BaseApiService, IExpenseService
{
    public ExpenseService(HttpClient http) : base(http) { }

    public Task<ApiResponse<List<ExpenseDto>>> GetAllAsync(
        DateTime? dateFrom = null, DateTime? dateTo = null, string? category = null, CancellationToken ct = default)
    {
        var q = "api/v1/expenses";
        var sep = '?';
        if (dateFrom.HasValue) { q += $"{sep}dateFrom={dateFrom.Value:yyyy-MM-dd}"; sep = '&'; }
        if (dateTo.HasValue)   { q += $"{sep}dateTo={dateTo.Value:yyyy-MM-dd}";     sep = '&'; }
        if (!string.IsNullOrEmpty(category)) q += $"{sep}category={Uri.EscapeDataString(category)}";
        return GetAsync<List<ExpenseDto>>(q, ct);
    }

    public Task<ApiResponse<ExpenseDto>> GetByIdAsync(long id, CancellationToken ct = default) =>
        GetAsync<ExpenseDto>($"api/v1/expenses/{id}", ct);

    public Task<ApiResponse<ExpenseDto>> CreateAsync(CreateExpenseRequest request, CancellationToken ct = default) =>
        PostAsync<ExpenseDto>("api/v1/expenses", request, ct);

    public Task<ApiResponse<ExpenseDto>> UpdateAsync(long id, UpdateExpenseRequest request, CancellationToken ct = default) =>
        PutAsync<ExpenseDto>($"api/v1/expenses/{id}", request, ct);

    public Task<ApiResponse<bool>> DeleteAsync(long id, CancellationToken ct = default) =>
        DeleteAsync<bool>($"api/v1/expenses/{id}", ct);
}
