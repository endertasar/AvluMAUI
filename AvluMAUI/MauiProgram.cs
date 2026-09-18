using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using AvluMAUI.Helpers;
using AvluMAUI.Services.Interfaces;
using AvluMAUI.Services.Implementations;
using AvluMAUI.ViewModels.Auth;
using AvluMAUI.ViewModels.Dashboard;
using AvluMAUI.ViewModels.Properties;
using AvluMAUI.ViewModels.Dues;
using AvluMAUI.ViewModels.Payments;
using AvluMAUI.ViewModels.Notifications;
using AvluMAUI.ViewModels.Reports;
using AvluMAUI.Views.Auth;
using AvluMAUI.Views.Dashboard;
using AvluMAUI.Views.Properties;
using AvluMAUI.Views.Dues;
using AvluMAUI.Views.Payments;
using AvluMAUI.Views.Notifications;
using AvluMAUI.Views.Reports;

namespace AvluMAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        StartupLogger.Init();
        StartupLogger.Step("CreateMauiApp begin");

        try
        {
            var builder = MauiApp.CreateBuilder();
            StartupLogger.Step("MauiApp.CreateBuilder() OK");

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "Inter");
                    fonts.AddFont("OpenSans-Semibold.ttf", "InterSemiBold");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            StartupLogger.Step("UseMauiApp + fonts OK");

            // HTTP Client
            builder.Services.AddSingleton(sp =>
            {
                var handler = new HttpClientHandler();
#if DEBUG
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
#endif
                return new HttpClient(handler)
                {
                    BaseAddress = new Uri(Constants.ApiBaseUrl),
                    Timeout = TimeSpan.FromSeconds(30)
                };
            });
            StartupLogger.Step("HttpClient registered");

            // Helpers
            builder.Services.AddSingleton<TokenManager>();
            StartupLogger.Step("TokenManager registered");

            // Services
            builder.Services.AddSingleton<IAuthService,         AuthService>();
            builder.Services.AddSingleton<IPropertyService,     PropertyService>();
            builder.Services.AddSingleton<IResidentService,     ResidentService>();
            builder.Services.AddSingleton<IDuesService,         DuesService>();
            builder.Services.AddSingleton<IChargeService,       ChargeService>();
            builder.Services.AddSingleton<IPaymentService,      PaymentService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IReportService,       ReportService>();
            StartupLogger.Step("Services registered");

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<SignupViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<PropertyListViewModel>();
            builder.Services.AddTransient<PropertyDetailViewModel>();
            builder.Services.AddTransient<PropertyAddEditViewModel>();
            builder.Services.AddTransient<AssignResidentViewModel>();
            builder.Services.AddTransient<DuesDefinitionListViewModel>();
            builder.Services.AddTransient<DuesDefinitionAddViewModel>();
            builder.Services.AddTransient<BulkChargeViewModel>();
            builder.Services.AddTransient<SingleChargeViewModel>();
            builder.Services.AddTransient<PaymentCollectViewModel>();
            builder.Services.AddTransient<PaymentListViewModel>();
            builder.Services.AddTransient<NotificationSendViewModel>();
            builder.Services.AddTransient<NotificationHistoryViewModel>();
            builder.Services.AddTransient<ReportsMenuViewModel>();
            builder.Services.AddTransient<MonthlyCollectionViewModel>();
            builder.Services.AddTransient<DebtAgingViewModel>();
            builder.Services.AddTransient<PropertySummaryViewModel>();
            builder.Services.AddTransient<OverdueViewModel>();
            StartupLogger.Step("ViewModels registered");

            // Shell
            builder.Services.AddSingleton<AppShell>();

            // Views
            builder.Services.AddTransient<SplashPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SignupPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<PropertyListPage>();
            builder.Services.AddTransient<PropertyDetailPage>();
            builder.Services.AddTransient<PropertyAddEditPage>();
            builder.Services.AddTransient<AssignResidentPage>();
            builder.Services.AddTransient<DuesDefinitionListPage>();
            builder.Services.AddTransient<DuesDefinitionAddPage>();
            builder.Services.AddTransient<BulkChargePage>();
            builder.Services.AddTransient<SingleChargePage>();
            builder.Services.AddTransient<PaymentCollectPage>();
            builder.Services.AddTransient<PaymentListPage>();
            builder.Services.AddTransient<NotificationSendPage>();
            builder.Services.AddTransient<NotificationHistoryPage>();
            builder.Services.AddTransient<ReportsMenuPage>();
            builder.Services.AddTransient<MonthlyCollectionPage>();
            builder.Services.AddTransient<DebtAgingPage>();
            builder.Services.AddTransient<PropertySummaryPage>();
            builder.Services.AddTransient<OverduePage>();
            StartupLogger.Step("Views registered");

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            StartupLogger.Step($"builder.Build() OK — log: {StartupLogger.LogPath}");
            return app;
        }
        catch (Exception ex)
        {
            StartupLogger.Fail("CreateMauiApp FATAL", ex);
            throw;
        }
    }
}
