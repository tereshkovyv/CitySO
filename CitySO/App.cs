using System.Windows;
using CitySO.UI.Views;
using CitySO.Views;

namespace CitySO;

public class App(MainWindow mainWindow) : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        MainWindow = mainWindow;
        mainWindow.Show();
        base.OnStartup(e);
    }
}