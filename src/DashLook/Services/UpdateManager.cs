using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;

namespace DashLook.Services;

/// <summary>
/// Checks GitHub Releases for a newer version and downloads + installs it.
/// Update source: https://api.github.com/repos/itrabbi24/DashLook/releases/latest
/// </summary>
public sealed class UpdateManager
{
    private const string ApiUrl    = "https://api.github.com/repos/itrabbi24/DashLook/releases/latest";
    private const string RepoOwner = "itrabbi24";
    private const string RepoName  = "DashLook";

    private static readonly HttpClient Http = new(new HttpClientHandler
    {
        AllowAutoRedirect = true,
    })
    {
        DefaultRequestHeaders =
        {
            { "User-Agent",  $"DashLook/{CurrentVersion}" },
            { "Accept",      "application/vnd.github.v3+json" },
        },
        Timeout = TimeSpan.FromSeconds(15),
    };

    public static Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);

    public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    /// <summary>
    /// Checks for a new release. Call on startup (silently) or when user clicks "Check for Updates".
    /// </summary>
    public async Task<UpdateInfo?> CheckAsync(bool silent = true)
    {
        try
        {
            var release = await Http.GetFromJsonAsync<GitHubRelease>(ApiUrl);
            if (release is null) return null;

            // Parse tag like "v1.2.0" → Version
            string raw = release.TagName.TrimStart('v');
            if (!Version.TryParse(raw, out var latest)) return null;

            if (latest <= CurrentVersion)
                return null; // already up to date

            // Find the best asset (prefer EXE, fall back to ZIP)
            var asset = release.Assets.FirstOrDefault(a => a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                     ?? release.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

            var info = new UpdateInfo
            {
                CurrentVersion  = CurrentVersion,
                LatestVersion   = latest,
                ReleaseNotes    = release.Body ?? "",
                ReleaseUrl      = release.HtmlUrl,
                DownloadUrl     = asset?.BrowserDownloadUrl,
                AssetName       = asset?.Name ?? "",
                PublishedAt     = release.PublishedAt,
            };

            UpdateAvailable?.Invoke(this, new UpdateAvailableEventArgs(info));
            return info;
        }
        catch
        {
            return null; // silently fail — network might be offline
        }
    }

    /// <summary>
    /// Downloads the update to a temp file and launches it, then quits DashLook.
    /// Progress 0–100 reported via the callback.
    /// </summary>
    public async Task DownloadAndInstallAsync(UpdateInfo info, Action<int> onProgress, CancellationToken token)
    {
        if (info.DownloadUrl is null)
            throw new InvalidOperationException("No download URL available for this release.");

        string tempPath = Path.Combine(Path.GetTempPath(),
            $"DashLook-Update-{info.LatestVersion}{Path.GetExtension(info.AssetName)}");

        // Download with progress
        using var response = await Http.GetAsync(info.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();

        long total  = response.Content.Headers.ContentLength ?? -1;
        long read   = 0;

        await using var dest   = File.Create(tempPath);
        await using var stream = await response.Content.ReadAsStreamAsync(token);

        var buffer = new byte[81920];
        int bytes;
        while ((bytes = await stream.ReadAsync(buffer, token)) > 0)
        {
            await dest.WriteAsync(buffer.AsMemory(0, bytes), token);
            read += bytes;
            if (total > 0)
                onProgress((int)(read * 100 / total));
        }

        dest.Close();
        onProgress(100);

        // Launch the installer / EXE
        Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });

        // Quit so the installer can replace files
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
            System.Windows.Application.Current.Shutdown());
    }

    // ── GitHub API models ─────────────────────────────────────────────────────

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]    public string TagName     { get; set; } = "";
        [JsonPropertyName("html_url")]    public string HtmlUrl     { get; set; } = "";
        [JsonPropertyName("body")]        public string? Body        { get; set; }
        [JsonPropertyName("published_at")]public DateTime PublishedAt { get; set; }
        [JsonPropertyName("assets")]      public List<GitHubAsset> Assets { get; set; } = new();
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]                 public string Name               { get; set; } = "";
        [JsonPropertyName("browser_download_url")] public string BrowserDownloadUrl { get; set; } = "";
        [JsonPropertyName("size")]                 public long   Size               { get; set; }
    }
}

public sealed class UpdateInfo
{
    public Version   CurrentVersion  { get; init; } = new();
    public Version   LatestVersion   { get; init; } = new();
    public string    ReleaseNotes    { get; init; } = "";
    public string    ReleaseUrl      { get; init; } = "";
    public string?   DownloadUrl     { get; init; }
    public string    AssetName       { get; init; } = "";
    public DateTime  PublishedAt     { get; init; }
    public bool      HasInstaller    => DownloadUrl is not null;
}

public sealed class UpdateAvailableEventArgs(UpdateInfo info) : EventArgs
{
    public UpdateInfo Info { get; } = info;
}
