using AvluMAUI.Views.Auth;
using AvluMAUI.Views.Properties;
using AvluMAUI.Views.Dues;
using AvluMAUI.Views.Payments;
using AvluMAUI.Views.Notifications;
using AvluMAUI.Views.Reports;

namespace AvluMAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    static void RegisterRoutes()
    {
        // Auth
        Routing.RegisterRoute("forgot", typeof(ForgotPasswordPage));
        Routing.RegisterRoute("signup", typeof(SignupPage));

        // Properties
        Routing.RegisterRoute("properties/detail",   typeof(PropertyDetailPage));
        Routing.RegisterRoute("properties/addedit",  typeof(PropertyAddEditPage));
        Routing.RegisterRoute("properties/assign",   typeof(AssignResidentPage));

        // Dues
        Routing.RegisterRoute("dues/definition/add", typeof(DuesDefinitionAddPage));
        Routing.RegisterRoute("dues/bulk",           typeof(BulkChargePage));
        Routing.RegisterRoute("dues/single",         typeof(SingleChargePage));

        // Payments
        Routing.RegisterRoute("payments/list",       typeof(PaymentListPage));

        // Notifications
        Routing.RegisterRoute("notifications/send",  typeof(NotificationSendPage));
        Routing.RegisterRoute("notifications/history", typeof(NotificationHistoryPage));

        // Reports
        Routing.RegisterRoute("reports/monthly",     typeof(MonthlyCollectionPage));
        Routing.RegisterRoute("reports/aging",       typeof(DebtAgingPage));
        Routing.RegisterRoute("reports/summary",     typeof(PropertySummaryPage));
        Routing.RegisterRoute("reports/overdue",     typeof(OverduePage));
    }
}
