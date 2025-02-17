using System.Net.Http;
using System.Text.Json;
using System.Diagnostics;
using System.Reflection;
using System.Security.Policy;
using BoosterBot.Resources;

namespace BoosterBot.Helpers;

public class GitHubRelease
{
    public string  tag_name { get; set; }
    public string html_url { get; set; }
    public string body { get; set; }
    public bool prerelease { get; set; }
}

public static class UpdateChecker
{
    private const string GITHUB_API_URL = "https://api.github.com/repos/little-fort/booster-bot/releases/latest";
    private static GitHubRelease _latestRelease;

    public static async Task<bool> CheckForUpdates()
    {
        try
        {
            using var client = new HttpClient();
            // GitHub API requires a user agent
            client.DefaultRequestHeaders.Add("User-Agent", "BoosterBot-UpdateChecker");

            var response = await client.GetStringAsync(GITHUB_API_URL);
            _latestRelease = JsonSerializer.Deserialize<GitHubRelease>(response);

            if (_latestRelease == null || string.IsNullOrEmpty(_latestRelease.tag_name))
                return false;

            // Get current version
            var assembly = Assembly.GetEntryAssembly();
            string currentVersion = null;
            if (assembly != null)
            {
                currentVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?.Split('+')[0];
            }
            else
            {
                // Handle null assembly (for example, throw exception or set a default value)
                return false;
            }

            // Compare versions (assuming semantic versioning without 'v' prefix)
            var latestVersion = _latestRelease.tag_name.TrimStart('v');
            return IsNewerVersion(latestVersion, currentVersion);
        }
        catch (Exception ex)
        {
            // Log the error but don't disrupt the application
            Console.WriteLine($"Update check failed: {ex.Message}", "logs\\update.txt");
            return false;
        }
    }

    private static bool IsNewerVersion(string latest, string current)
    {
        // Parse versions and compare
        if (Version.TryParse(latest, out Version latestVersion) &&
            Version.TryParse(current, out Version currentVersion))
        {
            return latestVersion > currentVersion;
        }
        return false;
    }

    public static void OpenReleasePage()
    {
        if (_latestRelease?.html_url != null)
        {
            try
            {
                Process.Start("rundll32", $"url.dll,FileProtocolHandler {_latestRelease.html_url}");
            }
            catch (Exception ex)
            {
                // Logger.Log($"Failed to open release page: {ex.Message}", "logs\\update.txt");
            }
        }
    }

    public static string GetUpdateMessage()
    {
        if (_latestRelease == null)
            return string.Empty;

        var body = _latestRelease.body;
        var lines = body.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var message = Strings.Update_NewVersion.Replace("%VALUE%", _latestRelease.tag_name);

        foreach (var line in lines)
            message += $"   {line}\n";

        return message;
    }
}
