using Microsoft.Maui.Controls;

namespace Avlu;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
