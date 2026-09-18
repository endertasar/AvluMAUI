using Avlu.ResidentApp.Views;
using Microsoft.Maui.Controls;

namespace Avlu.ResidentApp;

public partial class ResidentShell : Shell
{
    public ResidentShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("property-select", typeof(PropertySelectionPage));
        Routing.RegisterRoute("notification-detail", typeof(NotificationDetailPage));
    }
}
