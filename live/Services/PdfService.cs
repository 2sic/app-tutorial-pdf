using System;
using System.Web;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.IO;
using Newtonsoft.Json;
using System.Threading;

public class PdfService : Custom.Hybrid.Code14
{
  public const string chromeConnectionUrl = "http://localhost:9222";

  // ADJUST With folder name after browser has been downloaded from /Pdf/chrome-win/{chromeVersionName}
  public const string chromeVersionName = "Win64-884014";
  public async Task<byte[]> GetWebPageAsPdf(string url)
  {
    // Fetches chrome browser, when required

    var loadBrowser = false;
    try
    {
      if (Directory.GetFiles(GetChromeLocation()).Length == 0) {
        loadBrowser = true;
      }
    }
    catch (Exception ex)
    {
      loadBrowser = true;
    }

    if (loadBrowser) {
      await DownloadBrowser();
    }

    // Parameters for Puppeteer try/catch execution
    int retryCount = 3;
    TimeSpan delay = TimeSpan.FromSeconds(4);
    int currentRetry = 0;

    // Options with which Chrome will launch  
    var launchOptions = GetLaunchParams();
    var pdfStream = new byte[0];

    // Loop for the process execution
    for (; ; )
    {
      try
      {
        pdfStream = await GetPdfStream(url);

        // break loop after a pdf stream was reeturned successfully
        break;
      }
      catch (Exception ex)
      {
        Trace.TraceError("Operation Exception");

        currentRetry++;
        if (currentRetry > retryCount) { throw; }
      }

      try
      {
        // Kill chrome processes - fallback for suspension issues
        await KillChromeProcess();
      }
      catch (Exception ex) { }

      try
      {
        // Launch a new browser instance using the launch options 
        await LaunchChromeProcess(launchOptions);
      }
      catch (Exception ex) { }

      await Task.Delay(delay);
    }

    return pdfStream;
  }

  public async Task<byte[]> GetPdfStream(string url)
  {
    // Try connecting to an existing Browser using websockets
    var browser = await Puppeteer.ConnectAsync(new ConnectOptions { BrowserURL = chromeConnectionUrl });

    var pages = await browser.PagesAsync();
    var page = pages[0];

    await page.GoToAsync(url,
      new NavigationOptions
      {
        // Wait until all Networkrequests have finished loading
        WaitUntil = new[] { WaitUntilNavigation.Networkidle2 }
      });

    // write the PdfStream using a custom configuration (PdfOptions not required)
    return await page.PdfDataAsync(new PdfOptions { Format = PuppeteerSharp.Media.PaperFormat.A4, PrintBackground = true, DisplayHeaderFooter = true });
  }

  public async Task LaunchChromeProcess(LaunchOptions launchOptions)
  {
    var launchedBrowser = await Puppeteer.LaunchAsync(launchOptions);
    await launchedBrowser.NewPageAsync();
    File.WriteAllText(GetProcessLocation(), Kit.Convert.Json.ToJson(new { ProcessId = launchedBrowser.Process.Id }));
  }

  public async Task KillChromeProcess()
  {
    string processLocation = GetAppPdfFolderName() + "\\chrome-win\\chrome.exe";
    processLocation = processLocation.Replace("/", "\\");
    Process[] chromeProcesses = Process.GetProcessesByName("chrome");

    foreach (var p in chromeProcesses)
    {
      if (p.MainModule.FileName != processLocation)
      {
        continue;
      }
      p.Kill();
    }

    File.WriteAllText(GetProcessLocation(), Kit.Convert.Json.ToJson(new { ProcessId = 0 }));
  }

  public async Task DownloadBrowser()
  {
    var browserFetcherOptions = new BrowserFetcherOptions { Path = GetChromeLocation() };
    await new BrowserFetcher(browserFetcherOptions).DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
  }

  public LaunchOptions GetLaunchParams()
  {
    return new LaunchOptions
    {
      Headless = true,
      ExecutablePath = GetExecutionPath(),
      Args = new[] { "--no-sandbox", "--remote-debugging-port=9222" },
    };
  }

  public string GetAppPdfFolderName()
  {
    return App.PhysicalPath + "/" + "Pdf";
  }

  public string GetChromeLocation()
  {
    return GetAppPdfFolderName() + "/chrome-win";
  }

  public string GetExecutionPath()
  {
    return GetAppPdfFolderName() + "/chrome-win/" + chromeVersionName + "/chrome-win/chrome.exe";
  }

  public string GetProcessLocation()
  {
    return GetAppPdfFolderName() + "/process.json";
  }
}

class ChromiumProcess
{
  public int ProcessId;
}
