using AvluMAUI.Models;
using AvluMAUI.Models.Report;

namespace AvluMAUI.Services.Interfaces;

public interface IReportService
{
    Task<ApiResponse<DashboardDto>>              GetDashboardAsync(CancellationToken ct = default);
    Task<ApiResponse<MonthlyCollectionDto>>      GetMonthlyCollectionAsync(int year, int month, CancellationToken ct = default);
    Task<ApiResponse<List<IncomeExpenseMonthDto>>> GetIncomeExpenseAsync(int year, CancellationToken ct = default);
    Task<ApiResponse<DebtAgingDto>>              GetDebtAgingAsync(CancellationToken ct = default);
    Task<ApiResponse<List<PropertySummaryDto>>>  GetPropertySummaryAsync(CancellationToken ct = default);
    Task<ApiResponse<List<OverdueChargeDto>>>    GetOverdueChargesAsync(CancellationToken ct = default);
}
