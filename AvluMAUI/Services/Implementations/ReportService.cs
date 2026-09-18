using AvluMAUI.Models;
using AvluMAUI.Models.Report;
using AvluMAUI.Services.Base;
using AvluMAUI.Services.Interfaces;

namespace AvluMAUI.Services.Implementations;

public class ReportService : BaseApiService, IReportService
{
    public ReportService(HttpClient http) : base(http) { }

    public Task<ApiResponse<DashboardDto>> GetDashboardAsync(CancellationToken ct = default) =>
        GetAsync<DashboardDto>("api/v1/reports/dashboard", ct);

    public Task<ApiResponse<MonthlyCollectionDto>> GetMonthlyCollectionAsync(int year, int month, CancellationToken ct = default) =>
        GetAsync<MonthlyCollectionDto>($"api/v1/reports/monthly-collection?year={year}&month={month}", ct);

    public Task<ApiResponse<List<IncomeExpenseMonthDto>>> GetIncomeExpenseAsync(int year, CancellationToken ct = default) =>
        GetAsync<List<IncomeExpenseMonthDto>>($"api/v1/reports/income-expense?year={year}", ct);

    public Task<ApiResponse<DebtAgingDto>> GetDebtAgingAsync(CancellationToken ct = default) =>
        GetAsync<DebtAgingDto>("api/v1/reports/debt-aging", ct);

    public Task<ApiResponse<List<PropertySummaryDto>>> GetPropertySummaryAsync(CancellationToken ct = default) =>
        GetAsync<List<PropertySummaryDto>>("api/v1/reports/property-summary", ct);

    public Task<ApiResponse<List<OverdueChargeDto>>> GetOverdueChargesAsync(CancellationToken ct = default) =>
        GetAsync<List<OverdueChargeDto>>("api/v1/reports/overdue", ct);
}
