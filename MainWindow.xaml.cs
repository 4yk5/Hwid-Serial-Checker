using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace HwidSerialChecker;

public partial class MainWindow : Window
{
    private string? _latestReportPath;
    private readonly DispatcherTimer _ledTimer = new();
    private double _ledAngle = 0;
    private bool _isScanning = false;
    private UpdateInfo? _availableUpdate;

    public MainWindow()
    {
        InitializeComponent();

        _ledTimer.Interval = TimeSpan.FromMilliseconds(35);
        _ledTimer.Tick += LedTimer_Tick;
        _ledTimer.Start();

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        PlayIntroSplash();
        _ = ServiceGuard.CheckAndEnforceAsync(this);

        AddLog("INIT", "OG Community Hardware Auditor initialized.", "#94A3B8");
        AddLog("READY", "System ready. Click 'CHECK SERIALS' to record baseline.", "#4ADE80");

        try
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string originalReport = Path.Combine(desktop, "OG_OriginalSerials.html");

            if (File.Exists(originalReport) && ComparisonEngine.HasBaseline())
            {
                var items = ComparisonEngine.LoadBaseline();
                if (items != null && items.Count > 0)
                {
                    UpdateHardwareSnapshot(items);
                    SetBaselineStatus(true);
                    AddLog("READY", $"{items.Count} baseline serials loaded from memory.", "#E2E8F0");
                }
                else
                {
                    SetBaselineStatus(false);
                }
            }
            else
            {
                SetBaselineStatus(false);
            }
        }
        catch
        {
            SetBaselineStatus(false);
        }

        FindLatestDesktopReport();
        _ = CheckForUpdatesInBackgroundAsync();
    }

    private void PlayIntroSplash()
    {
        var sb = new Storyboard();

        var glowAnim = new DoubleAnimation
        {
            From = 0.45,
            To = 0.95,
            Duration = TimeSpan.FromMilliseconds(450),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(1)
        };
        Storyboard.SetTarget(glowAnim, SplashLogoGlow);
        Storyboard.SetTargetProperty(glowAnim, new PropertyPath(DropShadowEffect.OpacityProperty));
        sb.Children.Add(glowAnim);

        var fadeOut = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            BeginTime = TimeSpan.FromMilliseconds(850),
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(fadeOut, SplashOverlay);
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));
        sb.Children.Add(fadeOut);

        sb.Completed += (s, e) =>
        {
            SplashOverlay.Visibility = Visibility.Collapsed;
        };

        sb.Begin();
    }

    private async Task CheckForUpdatesInBackgroundAsync()
    {
        try
        {
            var info = await UpdateManager.CheckForUpdatesAsync();
            if (info != null)
            {
                _availableUpdate = info;
                Dispatcher.Invoke(() =>
                {
                    TxtUpdatePill.Text = $"UPDATE v{info.Version}";
                    BtnUpdatePill.Visibility = Visibility.Visible;
                    AddLog("UPDATE", $"New version v{info.Version} is available on GitHub!", "#4ADE80");
                });
            }
        }
        catch { }
    }

    private async void BtnUpdatePill_Click(object sender, MouseButtonEventArgs e)
    {
        if (_availableUpdate == null || string.IsNullOrEmpty(_availableUpdate.DownloadUrl)) return;

        BtnUpdatePill.IsEnabled = false;
        AddLog("UPDATE", $"Downloading v{_availableUpdate.Version} from GitHub...", "#60A5FA");

        try
        {
            await UpdateManager.DownloadAndApplyUpdateAsync(_availableUpdate.DownloadUrl, progress =>
            {
                Dispatcher.Invoke(() =>
                {
                    TxtUpdatePill.Text = $"DOWNLOADING {progress}%";
                });
            });
        }
        catch (Exception ex)
        {
            BtnUpdatePill.IsEnabled = true;
            TxtUpdatePill.Text = $"UPDATE v{_availableUpdate.Version}";
            AddLog("ERROR", $"Update failed: {ex.Message}", "#F87171");
            MessageBox.Show($"Update download failed:\n{ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SetBaselineStatus(bool isChecked)
    {
        if (isChecked)
        {
            TxtBaselinePill.Text = "CHECKED";
            TxtBaselinePill.Foreground = new SolidColorBrush(Color.FromRgb(74, 222, 128));
            BorderBaselinePill.Background = new SolidColorBrush(Color.FromArgb(45, 74, 222, 128));
            BorderBaselinePill.BorderBrush = new SolidColorBrush(Color.FromArgb(120, 74, 222, 128));
        }
        else
        {
            TxtBaselinePill.Text = "NOT CHECKED";
            TxtBaselinePill.Foreground = new SolidColorBrush(Color.FromRgb(248, 113, 113));
            BorderBaselinePill.Background = new SolidColorBrush(Color.FromArgb(50, 239, 68, 68));
            BorderBaselinePill.BorderBrush = new SolidColorBrush(Color.FromArgb(120, 239, 68, 68));
        }
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

    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void LedTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            _ledAngle += 0.12;
            if (_ledAngle > Math.PI * 2) _ledAngle -= Math.PI * 2;

            double pulse = (Math.Sin(_ledAngle) + 1.0) * 0.5;
            LedDot.Opacity = 0.4 + (pulse * 0.6);
        }
        catch { }
    }

    private async void CheckCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (_isScanning) return;
        if (!await ServiceGuard.CheckAndEnforceAsync(this)) return;
        SetScanningState(true, "SCANNING...");

        AddLog("SCAN", "Querying hardware identifiers (WMI, EDID, Registry)...", "#60A5FA");

        try
        {
            var items = await Task.Run(() => HardwareScanner.ScanAll());

            ComparisonEngine.SaveBaseline(items);
            UpdateHardwareSnapshot(items);

            SetBaselineStatus(true);

            AddLog("SUCCESS", $"Captured {items.Count} hardware identifiers.", "#4ADE80");

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = "OG_OriginalSerials.html";
            string fullPath = Path.Combine(desktopPath, fileName);

            string html = ReportExporter.GenerateSingleScanHtml(items);
            await SafeWriteReportFileAsync(fullPath, html);

            _latestReportPath = fullPath;
            AddLog("REPORT", $"Saved to Desktop: {fileName}", "#FFFFFF");

            OpenBrowser(fullPath);
        }
        catch (Exception ex)
        {
            AddLog("ERROR", $"Scan error: {ex.Message}", "#F87171");
            MessageBox.Show($"Error during scan:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetScanningState(false, "ENGINE READY");
        }
    }

    private async void CompareCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (_isScanning) return;
        if (!await ServiceGuard.CheckAndEnforceAsync(this)) return;

        if (!ComparisonEngine.HasBaseline())
        {
            AddLog("WARN", "No baseline found! Click 'CHECK SERIALS' first.", "#FB923C");
            MessageBox.Show("No original serials found!\n\nPlease click 'CHECK SERIALS' first to capture your original hardware IDs.",
                "Baseline Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SetScanningState(true, "COMPARING...");
        AddLog("SCAN", "Querying new hardware IDs for spoof verification...", "#FB923C");

        try
        {
            var oldItems = ComparisonEngine.LoadBaseline();
            if (oldItems == null || oldItems.Count == 0)
            {
                AddLog("ERROR", "Failed to load baseline file.", "#F87171");
                return;
            }

            var newItems = await Task.Run(() => HardwareScanner.ScanAll());
            var comparison = ComparisonEngine.Compare(oldItems, newItems);
            UpdateHardwareSnapshot(newItems);

            int changedCount = comparison.Count(c => c.IsChanged);

            if (changedCount > 0)
            {
                TxtResultPill.Text = $"{changedCount} SPOOFED / CHANGED";
                TxtResultPill.Foreground = new SolidColorBrush(Color.FromRgb(74, 222, 128));
                BorderResultPill.BorderBrush = new SolidColorBrush(Color.FromArgb(90, 74, 222, 128));
                BorderResultPill.Background = new SolidColorBrush(Color.FromArgb(30, 74, 222, 128));
                AddLog("SPOOF", $"[!] SPOOF VERIFIED: {changedCount} SERIALS CHANGED!", "#4ADE80");
            }
            else
            {
                TxtResultPill.Text = "NO CHANGES (IDENTICAL)";
                TxtResultPill.Foreground = new SolidColorBrush(Color.FromRgb(248, 113, 113));
                BorderResultPill.BorderBrush = new SolidColorBrush(Color.FromArgb(90, 239, 68, 68));
                BorderResultPill.Background = new SolidColorBrush(Color.FromArgb(30, 239, 68, 68));
                AddLog("MATCH", "No changes detected. All hardware IDs match.", "#F87171");
            }

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = "OG_NewSerials.html";
            string fullPath = Path.Combine(desktopPath, fileName);

            string html = ReportExporter.GenerateComparisonHtml(comparison);
            await SafeWriteReportFileAsync(fullPath, html);

            _latestReportPath = fullPath;
            AddLog("REPORT", $"Saved to Desktop: {fileName}", "#FFFFFF");

            OpenBrowser(fullPath);
        }
        catch (Exception ex)
        {
            AddLog("ERROR", $"Comparison error: {ex.Message}", "#F87171");
            MessageBox.Show($"Error during comparison:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetScanningState(false, "ENGINE READY");
        }
    }

    private static async Task SafeWriteReportFileAsync(string fullPath, string content)
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(fs, Encoding.UTF8);
                await writer.WriteAsync(content);
                return;
            }
            catch (IOException)
            {
                if (attempt == 4) throw;
                await Task.Delay(150);
            }
        }
    }

    private void SetScanningState(bool scanning, string statusText)
    {
        _isScanning = scanning;
        TxtStatusPill.Text = statusText;
        Cursor = scanning ? Cursors.Wait : Cursors.Arrow;
    }

    private void OpenReport_Click(object sender, RoutedEventArgs e)
    {
        FindLatestDesktopReport();
        if (!string.IsNullOrEmpty(_latestReportPath) && File.Exists(_latestReportPath))
        {
            OpenBrowser(_latestReportPath);
            AddLog("REPORT", $"Opened: {Path.GetFileName(_latestReportPath)}", "#FFFFFF");
        }
        else
        {
            MessageBox.Show("No report found on Desktop yet.\nPlease run a scan first.",
                "No Report", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void UpdateHardwareSnapshot(List<HardwareItem> items)
    {
        try
        {
            var mb = items.FirstOrDefault(i => i.Category.Contains("Baseboard") && i.Property.Contains("Product"))?.Value
                     ?? items.FirstOrDefault(i => i.Category.Contains("Baseboard") && i.Property.Contains("Serial"))?.Value ?? "N/A";

            var cpu = items.FirstOrDefault(i => i.Category.Contains("Processor") && i.Property.Contains("Type"))?.Value ?? "N/A";
            var gpu = items.FirstOrDefault(i => i.Category.Contains("GPU") && i.Property.Contains("Name"))?.Value ?? "N/A";
            var disk = items.FirstOrDefault(i => i.Category.Contains("Disk") && i.Property.Contains("MODEL"))?.Value ?? "N/A";
            var uuid = items.FirstOrDefault(i => i.Property.Contains("UUID"))?.Value ?? "N/A";

            TxtMbValue.Text = mb;
            TxtCpuValue.Text = cpu;
            TxtGpuValue.Text = gpu;
            TxtDiskValue.Text = disk;
            TxtUuidValue.Text = uuid;
        }
        catch { }
    }

    private void AddLog(string tag, string message, string hexColor)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");

            var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(64) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(58) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var tsBlock = new TextBlock
            {
                Text = timestamp,
                FontSize = 10,
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(tsBlock, 0);

            var tagBorder = new Border
            {
                CornerRadius = new CornerRadius(5),
                Background = new SolidColorBrush(Color.FromArgb(35, 255, 255, 255)),
                Height = 18,
                Width = 52,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var tagBlock = new TextBlock
            {
                Text = tag,
                FontSize = 8.5,
                FontWeight = FontWeights.ExtraBold,
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(hexColor)!,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            tagBorder.Child = tagBlock;
            Grid.SetColumn(tagBorder, 1);

            var msgBlock = new TextBlock
            {
                Text = message,
                FontSize = 10.5,
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString(hexColor)!,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(msgBlock, 2);

            row.Children.Add(tsBlock);
            row.Children.Add(tagBorder);
            row.Children.Add(msgBlock);

            TerminalLogStack.Children.Add(row);
            TerminalScroller.ScrollToBottom();
        }
        catch { }
    }

    private void FindLatestDesktopReport()
    {
        try
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string newReport = Path.Combine(desktop, "OG_NewSerials.html");
            string origReport = Path.Combine(desktop, "OG_OriginalSerials.html");

            if (File.Exists(newReport) && File.Exists(origReport))
            {
                _latestReportPath = File.GetLastWriteTime(newReport) >= File.GetLastWriteTime(origReport)
                    ? newReport
                    : origReport;
            }
            else if (File.Exists(newReport))
            {
                _latestReportPath = newReport;
            }
            else if (File.Exists(origReport))
            {
                _latestReportPath = origReport;
            }
            else
            {
                _latestReportPath = Directory.GetFiles(desktop, "OG_*.html")
                    .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                    .FirstOrDefault();
            }
        }
        catch { }
    }

    private void OpenBrowser(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch { }
    }

    public void ShowUpdateRequired(string? newVersion = null)
    {
        Dispatcher.Invoke(() =>
        {
            SplashOverlay.Visibility = Visibility.Collapsed;
            string latest = string.IsNullOrWhiteSpace(newVersion) ? "New Version Available" : $"Latest: {newVersion}";
            TxtUpdateRequiredVersion.Text = $"Current: {ServiceGuard.CurrentVersion}  •  {latest}";
            UpdateRequiredOverlay.Visibility = Visibility.Visible;
        });
    }

    private void BtnDownloadUpdateRequired_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(ServiceGuard.ReleasesUrl) { UseShellExecute = true });
        }
        catch { }
        Application.Current.Shutdown();
    }

    private void BtnExitApp_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
