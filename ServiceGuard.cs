using System.Diagnostics;
using System.Net.Http;
using System.Windows;

namespace HwidSerialChecker;

public class ServiceStatusResult
{
    public bool IsAllowed { get; set; } = true;
    public bool RequiresUpdate { get; set; } = false;
    public string? AvailableVersion { get; set; }
}

public static class ServiceGuard
{
    public const string CurrentVersion = "v1";
    public const string ReleasesUrl = "https://github.com/4yk5/Hwid-Serial-Checker/releases";
    private const string StatusUrl = "https://raw.githubusercontent.com/4yk5/Hwid-Serial-Checker/main/status.txt";

    public static async Task<ServiceStatusResult> CheckStatusAsync()
    {
        var result = new ServiceStatusResult();
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            string url = $"{StatusUrl}?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return result;
            }

            string content = (await response.Content.ReadAsStringAsync()).Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                return result;
            }

            if (content.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                result.IsAllowed = false;
                return result;
            }

            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            bool? thisVersionActive = null;
            string? foundNewVersion = null;

            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

                int colonIdx = line.IndexOf(':');
                if (colonIdx > 0)
                {
                    string ver = line.Substring(0, colonIdx).Trim();
                    string val = line.Substring(colonIdx + 1).Trim().ToLowerInvariant();
                    bool isValTrue = val == "true" || val == "1" || val == "ture" || val == "enabled";

                    if (ver.Equals(CurrentVersion, StringComparison.OrdinalIgnoreCase))
                    {
                        thisVersionActive = isValTrue;
                    }
                    else if (isValTrue)
                    {
                        foundNewVersion = ver;
                    }
                }
            }

            if (thisVersionActive.HasValue)
            {
                if (!thisVersionActive.Value)
                {
                    result.IsAllowed = false;
                    result.RequiresUpdate = true;
                    result.AvailableVersion = foundNewVersion;
                }
                else
                {
                    result.IsAllowed = true;
                    result.RequiresUpdate = false;
                }
            }
        }
        catch { }

        return result;
    }

    public static async Task<bool> CheckAndEnforceAsync(MainWindow? window = null)
    {
        var status = await CheckStatusAsync();
        if (status.RequiresUpdate)
        {
            if (window != null)
            {
                window.ShowUpdateRequired(status.AvailableVersion);
            }
            else
            {
                var prompt = MessageBox.Show(
                    $"OG COMMUNITY SERIAL CHECKER\n\nVersion {CurrentVersion} has been discontinued. A newer version is available.\n\nOpen GitHub Releases to download the latest update?",
                    "Update Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (prompt == MessageBoxResult.Yes)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(ReleasesUrl) { UseShellExecute = true });
                    }
                    catch { }
                }
                Environment.Exit(0);
            }
            return false;
        }

        if (!status.IsAllowed)
        {
            MessageBox.Show(
                "OG COMMUNITY SERIAL CHECKER\n\nThis application has been temporarily disabled by the administrator.\nPlease check back later.",
                "Service Suspended",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            Environment.Exit(0);
            return false;
        }

        return true;
    }
}
