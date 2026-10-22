using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace HwidSerialChecker;

public class UpdateInfo
{
    public string Version { get; set; } = "1.0.0";
    public string DownloadUrl { get; set; } = string.Empty;
    public string Changelog { get; set; } = string.Empty;
}

public static class UpdateManager
{
    public const string CurrentVersion = "v1";

    public static string GitHubOwner { get; set; } = "4yk5";
    public static string GitHubRepo { get; set; } = "Hwid-Serial-Checker";

    public static string RawVersionUrl =>
        $"https://raw.githubusercontent.com/{GitHubOwner}/{GitHubRepo}/main/version.json";

    public static string GitHubApiReleasesUrl =>
        $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";

    public static async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(6);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OG-SerialChecker-Updater");

            try
            {
                var response = await client.GetAsync(RawVersionUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var info = JsonSerializer.Deserialize<UpdateInfo>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (info != null && !string.IsNullOrWhiteSpace(info.DownloadUrl) && IsNewerVersion(info.Version, CurrentVersion))
                    {
                        return info;
                    }
                }
            }
            catch { }

            try
            {
                var apiResponse = await client.GetAsync(GitHubApiReleasesUrl);
                if (apiResponse.IsSuccessStatusCode)
                {
                    var apiJson = await apiResponse.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(apiJson);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("tag_name", out var tagElem))
                    {
                        string remoteTag = tagElem.GetString() ?? "";
                        if (IsNewerVersion(remoteTag, CurrentVersion))
                        {
                            string downloadUrl = "";
                            if (root.TryGetProperty("assets", out var assets) && assets.GetArrayLength() > 0)
                            {
                                foreach (var asset in assets.EnumerateArray())
                                {
                                    string name = asset.GetProperty("name").GetString() ?? "";
                                    if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                    {
                                        downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? "";
                                        break;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(downloadUrl))
                            {
                                string body = root.TryGetProperty("body", out var b) ? b.GetString() ?? "" : "";
                                return new UpdateInfo
                                {
                                    Version = remoteTag.TrimStart('v', 'V'),
                                    DownloadUrl = downloadUrl,
                                    Changelog = body
                                };
                            }
                        }
                    }
                }
            }
            catch { }
        }
        catch { }

        return null;
    }

    public static bool IsNewerVersion(string remoteVersion, string localVersion)
    {
        string rClean = remoteVersion.Trim().TrimStart('v', 'V');
        string lClean = localVersion.Trim().TrimStart('v', 'V');

        if (int.TryParse(rClean, out int rInt) && int.TryParse(lClean, out int lInt))
        {
            return rInt > lInt;
        }

        if (Version.TryParse(rClean, out var rVer) && Version.TryParse(lClean, out var lVer))
        {
            return rVer > lVer;
        }

        return string.Compare(rClean, lClean, StringComparison.OrdinalIgnoreCase) > 0;
    }

    public static async Task DownloadAndApplyUpdateAsync(string downloadUrl, Action<int> onProgress)
    {
        string currentExe = Process.GetCurrentProcess().MainModule?.FileName
            ?? Path.Combine(AppContext.BaseDirectory, "HwidSerialChecker.exe");

        string dir = Path.GetDirectoryName(currentExe)!;
        string tempExe = Path.Combine(dir, "HwidSerialChecker_update.exe");

        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromMinutes(3);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("OG-SerialChecker-Updater");

            using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? -1;
            using var stream = await response.Content.ReadAsStreamAsync();
            using var fs = new FileStream(tempExe, FileMode.Create, FileAccess.Write, FileShare.None);

            byte[] buffer = new byte[81920];
            long totalRead = 0;
            int read;

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fs.WriteAsync(buffer, 0, read);
                totalRead += read;
                if (totalBytes > 0)
                {
                    int pct = (int)((totalRead * 100) / totalBytes);
                    onProgress(Math.Min(99, pct));
                }
            }
        }

        onProgress(100);

        string cmdArgs = $"/c timeout /t 1 /nobreak >nul & move /y \"{tempExe}\" \"{currentExe}\" & start \"\" \"{currentExe}\"";

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = cmdArgs,
            CreateNoWindow = true,
            UseShellExecute = false
        });

        Environment.Exit(0);
    }
}
