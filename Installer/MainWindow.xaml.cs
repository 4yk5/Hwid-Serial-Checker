using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace OGCommunityInstaller;

public partial class MainWindow : Window
{
    private readonly string _installDir;
    private bool _isInstalled = false;

    public MainWindow()
    {
        InitializeComponent();

        _installDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs",
            "OG Community Serial Checker");

        TxtInstallPath.Text = _installDir;
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try
            {
                DragMove();
            }
            catch { }
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void BtnAction_Click(object sender, RoutedEventArgs e)
    {
        if (_isInstalled)
        {
            if (ChkLaunchAfter.IsChecked == true)
            {
                string exePath = Path.Combine(_installDir, "HwidSerialChecker.exe");
                if (File.Exists(exePath))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(exePath)
                        {
                            WorkingDirectory = _installDir,
                            UseShellExecute = true
                        });
                    }
                    catch { }
                }
            }
            Close();
            return;
        }

        BtnAction.IsEnabled = false;
        TxtPercent.Visibility = Visibility.Visible;

        try
        {
            UpdateProgress(10, "Creating installation directory...");
            await Task.Delay(200);

            if (!Directory.Exists(_installDir))
            {
                Directory.CreateDirectory(_installDir);
            }

            UpdateProgress(35, "Extracting core binaries...");
            await Task.Delay(250);

            string exePath = Path.Combine(_installDir, "HwidSerialChecker.exe");
            string icoPath = Path.Combine(_installDir, "app.ico");

            ExtractEmbeddedResource("OGCommunityInstaller.Payload.app_payload.bin", exePath);

            UpdateProgress(60, "Extracting icons and assets...");
            await Task.Delay(200);

            ExtractEmbeddedResource("OGCommunityInstaller.Payload.app.ico", icoPath);

            UpdateProgress(75, "Creating system shortcuts...");
            await Task.Delay(200);

            if (ChkDesktopShortcut.IsChecked == true)
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string lnk = Path.Combine(desktop, "OG Community Serial Checker.lnk");
                CreateShortcut(lnk, exePath, _installDir, icoPath, "OG Community Serial Checker");
            }

            if (ChkStartMenuShortcut.IsChecked == true)
            {
                string startMenu = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Start Menu\Programs\OG Community");

                if (!Directory.Exists(startMenu))
                {
                    Directory.CreateDirectory(startMenu);
                }

                string lnk = Path.Combine(startMenu, "OG Community Serial Checker.lnk");
                CreateShortcut(lnk, exePath, _installDir, icoPath, "OG Community Serial Checker");
            }

            UpdateProgress(90, "Registering in Windows Apps & Features...");
            await Task.Delay(200);

            RegisterUninstaller(icoPath);

            UpdateProgress(100, "Installation completed successfully!");
            TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(74, 222, 128));

            _isInstalled = true;
            TxtBtnAction.Text = ChkLaunchAfter.IsChecked == true ? "LAUNCH APPLICATION" : "FINISH";
            BtnAction.IsEnabled = true;
        }
        catch (Exception ex)
        {
            TxtStatus.Text = $"Error: {ex.Message}";
            TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(248, 113, 113));
            BtnAction.IsEnabled = true;
        }
    }

    private void UpdateProgress(int percent, string message)
    {
        InstallProgress.Value = percent;
        TxtPercent.Text = $"{percent}%";
        TxtStatus.Text = message;
    }

    private static void ExtractEmbeddedResource(string resourceName, string outputPath)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded resource not found: {resourceName}");

        using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fs);
    }

    private static void CreateShortcut(string shortcutPath, string targetPath, string workingDir, string iconPath, string description)
    {
        try
        {
            Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;

            dynamic? shell = Activator.CreateInstance(shellType);
            if (shell == null) return;

            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = workingDir;
            shortcut.IconLocation = $"{iconPath},0";
            shortcut.Description = description;
            shortcut.Save();
        }
        catch { }
    }

    private void RegisterUninstaller(string icoPath)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\OGCommunitySerialChecker");
            if (key != null)
            {
                key.SetValue("DisplayName", "OG Community Serial Checker");
                key.SetValue("DisplayVersion", "1.0.0");
                key.SetValue("Publisher", "OG Community");
                key.SetValue("DisplayIcon", icoPath);
                key.SetValue("InstallLocation", _installDir);
                key.SetValue("UninstallString", $"cmd.exe /c rmdir /s /q \"{_installDir}\" & del \"%userprofile%\\Desktop\\OG Community Serial Checker.lnk\"");
                key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
            }
        }
        catch { }
    }
}
