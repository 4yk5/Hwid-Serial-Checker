using System.IO;
using System.Windows;

namespace HwidSerialChecker;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (s, args) =>
        {
            try
            {
                File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "wpf_crash.log"),
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Dispatcher Exception: {args.Exception}\n\n");
            }
            catch { }
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            try
            {
                File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "wpf_crash_domain.log"),
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Domain Exception: {args.ExceptionObject}\n\n");
            }
            catch { }
        };

        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            try
            {
                File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "wpf_crash_task.log"),
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Task Exception: {args.Exception}\n\n");
            }
            catch { }
            args.SetObserved();
        };

        try
        {
            var win = new MainWindow();
            win.Show();
        }
        catch (Exception ex)
        {
            File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "wpf_crash.log"),
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Startup Error: {ex}\n\n");
            MessageBox.Show($"Application could not start:\n{ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
