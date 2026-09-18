using AvluMAUI.Models;
using AvluMAUI.Models.Expense;

namespace AvluMAUI.Services.Interfaces;

public interface IExpenseService
{
    Task<ApiResponse<List<ExpenseDto>>> GetAllAsync(DateTime? dateFrom = null, DateTime? dateTo = null, string? category = null, CancellationToken ct = default);
    Task<ApiResponse<ExpenseDto>>       GetByIdAsync(long id, CancellationToken ct = default);
    Task<ApiResponse<ExpenseDto>>       CreateAsync(CreateExpenseRequest request, CancellationToken ct = default);
    Task<ApiResponse<ExpenseDto>>       UpdateAsync(long id, UpdateExpenseRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>>             DeleteAsync(long id, CancellationToken ct = default);
}
