using Microsoft.Maui.Controls;
using Avlu.AdminApp;

namespace Avlu;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services, AppShell shell)
    {
        _services = services;
        InitializeComponent();
        MainPage = shell;
    }

    public void GoToAdminShell()
    {
        MainPage = _services.GetRequiredService<AdminShell>();
    }
}
