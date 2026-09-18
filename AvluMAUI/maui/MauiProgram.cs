using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using AvluMAUI;
using AvluMAUI.Helpers;
using AvluMAUI.Services.Interfaces;
using AvluMAUI.Services.Implementations;
using Avlu.AdminApp;
using Avlu.AdminApp.ViewModels;
using Avlu.AdminApp.Views;
using Avlu.Views;

namespace Avlu;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Inter-Regular.ttf",   "Inter");
                fonts.AddFont("Inter-SemiBold.ttf",  "InterSemiBold");
                fonts.AddFont("OpenSans-Regular.ttf",  "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // HTTP client — dev: accept self-signed cert
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
                Timeout     = TimeSpan.FromSeconds(30)
            };
        });

        // Helpers
        builder.Services.AddSingleton<TokenManager>();

        // Services
        builder.Services.AddSingleton<IAuthService,         AuthService>();
        builder.Services.AddSingleton<IPropertyService,     PropertyService>();
        builder.Services.AddSingleton<IResidentService,     ResidentService>();
        builder.Services.AddSingleton<IDuesService,         DuesService>();
        builder.Services.AddSingleton<IChargeService,       ChargeService>();
        builder.Services.AddSingleton<IPaymentService,      PaymentService>();
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.AddSingleton<IReportService,       ReportService>();
        builder.Services.AddSingleton<IExpenseService,      ExpenseService>();
        builder.Services.AddSingleton<IExtraPaymentService, ExtraPaymentService>();

        // Admin ViewModels
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<PropertyListViewModel>();
        builder.Services.AddTransient<PropertyDetailViewModel>();
        builder.Services.AddTransient<CollectPropertyViewModel>();
        builder.Services.AddTransient<CollectDebtViewModel>();
        builder.Services.AddTransient<CollectFormViewModel>();

        // Shells
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<AdminShell>();

        // Auth views
        builder.Services.AddTransient<LoginPage>();

        // Admin views
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<PropertyListPage>();
        builder.Services.AddTransient<PropertyDetailPage>();
        builder.Services.AddTransient<PropertyFormPage>();
        builder.Services.AddTransient<CollectPropertyPage>();
        builder.Services.AddTransient<CollectDebtPage>();
        builder.Services.AddTransient<CollectFormPage>();
        builder.Services.AddTransient<CollectSuccessPage>();
        builder.Services.AddTransient<DuesDefinitionPage>();
        builder.Services.AddTransient<ExtraPaymentsListPage>();
        builder.Services.AddTransient<ExtraPaymentFormPage>();
        builder.Services.AddTransient<ExtraPaymentDetailPage>();
        builder.Services.AddTransient<ExpensesListPage>();
        builder.Services.AddTransient<ExpenseFormPage>();
        builder.Services.AddTransient<NotificationSendPage>();
        builder.Services.AddTransient<ReportsPage>();
        builder.Services.AddTransient<MorePage>();
        builder.Services.AddTransient<SubUsersPage>();
        builder.Services.AddTransient<SubUserFormPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
