using Avlu.AdminApp.Views;
using Microsoft.Maui.Controls;

namespace Avlu.AdminApp;

public partial class AdminShell : Shell
{
    public AdminShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("property-detail", typeof(PropertyDetailPage));
        Routing.RegisterRoute("property-form", typeof(PropertyFormPage));
        Routing.RegisterRoute("dues", typeof(DuesDefinitionPage));
        Routing.RegisterRoute("collect-debt", typeof(CollectDebtPage));
        Routing.RegisterRoute("collect-form", typeof(CollectFormPage));
        Routing.RegisterRoute("collect-success", typeof(CollectSuccessPage));
        Routing.RegisterRoute("extra", typeof(ExtraPaymentsListPage));
        Routing.RegisterRoute("extra-form", typeof(ExtraPaymentFormPage));
        Routing.RegisterRoute("extra-detail", typeof(ExtraPaymentDetailPage));
        Routing.RegisterRoute("expenses", typeof(ExpensesListPage));
        Routing.RegisterRoute("expense-form", typeof(ExpenseFormPage));
        Routing.RegisterRoute("notify", typeof(NotificationSendPage));
        Routing.RegisterRoute("subusers", typeof(SubUsersPage));
        Routing.RegisterRoute("subuser-form", typeof(SubUserFormPage));
    }
}
