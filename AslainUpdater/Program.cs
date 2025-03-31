using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string jsonUrl = "https://aslain.com/update_checker/WoT_installer.json";
        string downloadBaseUrl = "https://modp.wgcdn.co/media/mod_files/Aslains_WoT_Modpack_Installer_v.";

        string? latestVersion = await GetLatestVersion(jsonUrl);
        if (string.IsNullOrEmpty(latestVersion))
        {
            Console.WriteLine("Failed to retrieve installer version information.");
            return;
        }

        int lastDotIndex = latestVersion.LastIndexOf('.');
        if (lastDotIndex == -1)
        {
            Console.WriteLine("Invalid version format.");
            return;
        }

        string gameVersion = latestVersion[..lastDotIndex];
        string modVersion = latestVersion[(lastDotIndex + 1)..];
        string formattedVersion = $"{gameVersion}_{modVersion}";

        string fileName = $"Aslains_WoT_Modpack_Installer_v{formattedVersion}.exe";
        string downloadUrl = $"{downloadBaseUrl}{formattedVersion}.exe";
        string exePath = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Combine(exePath, fileName);

        // Download the file
        using (HttpClient client = new HttpClient())
        {
            try
            {
                Console.WriteLine("Downloading file...");
                byte[] fileBytes = await client.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(filePath, fileBytes);
                Console.WriteLine($"File downloaded to: {filePath}");

                // Run the installer in silent mode
                Console.WriteLine("Starting installer...");
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = filePath,
                        Arguments = "/SILENT",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                Console.WriteLine("Installation completed.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }
    }

    static async Task<string?> GetLatestVersion(string jsonUrl)
    {
        try
        {
            using HttpClient client = new HttpClient();
            string jsonResponse = await client.GetStringAsync(jsonUrl);
            var jsonDoc = JsonDocument.Parse(jsonResponse);
            return jsonDoc.RootElement.GetProperty("installer").GetProperty("version").GetString();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error retrieving version: {e.Message}");
            return null;
        }
    }
}
