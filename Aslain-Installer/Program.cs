using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using PuppeteerSharp;

class Program
{
    static async Task Main(string[] args)
    {
        string pageUrl = "https://wgmods.net/46/";
        string latestVersionUrl = await GetLatestVersionUrl(pageUrl);

        if (!string.IsNullOrEmpty(latestVersionUrl))
        {
            string fileName = Path.GetFileName(latestVersionUrl);
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(exePath, fileName);

            // Download the file
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Console.WriteLine("Downloading file...");
                    byte[] fileBytes = await client.GetByteArrayAsync(latestVersionUrl);
                    await File.WriteAllBytesAsync(filePath, fileBytes);
                    Console.WriteLine($"File downloaded successfully to: {filePath}");

                    // Run the installer silently
                    Console.WriteLine("Starting silent installation...");
                    Process process = new Process();
                    process.StartInfo.FileName = filePath;
                    process.StartInfo.Arguments = "/SILENT"; // Silent install argument
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
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
        else
        {
            Console.WriteLine("Could not find the latest version download link.");
        }
    }

    static async Task<string> GetLatestVersionUrl(string pageUrl)
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        await using var page = await browser.NewPageAsync();
        await page.GoToAsync(pageUrl);

        try
        {
            // Wait for the element to be loaded
            await page.WaitForSelectorAsync("a.ModDetails_hidden--2Rtru");

            // Get the href attribute of the link
            var linkElement = await page.QuerySelectorAsync("a.ModDetails_hidden--2Rtru");
            var href = await linkElement.EvaluateFunctionAsync<string>("el => el.href");

            return href;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred while fetching the latest version URL: {e.Message}");
            return null;
        }
    }
}