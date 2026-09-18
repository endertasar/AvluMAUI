using AvluMAUI.Helpers;

namespace AvluMAUI;

public partial class App : Application
{
    private readonly TokenManager _tokenManager;
    private readonly AppShell _shell;

    public App(TokenManager tokenManager, AppShell shell)
    {
        StartupLogger.Step("App ctor begin");
        try
        {
            InitializeComponent();
            StartupLogger.Step("App.InitializeComponent OK");
        }
        catch (Exception ex)
        {
            StartupLogger.Fail("App.InitializeComponent", ex);
            throw;
        }

        _tokenManager = tokenManager;
        _shell = shell;

        // Global hata yakalayıcılar
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            StartupLogger.Fail("AppDomain.UnhandledException", (Exception)e.ExceptionObject);

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            StartupLogger.Fail("UnobservedTaskException", e.Exception);
            e.SetObserved();
        };

        StartupLogger.Step("App ctor done");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        StartupLogger.Step("CreateWindow");
        return new Window(_shell);
    }

    protected override void OnStart()
    {
        base.OnStart();
        StartupLogger.Step("App.OnStart");
    }
}
