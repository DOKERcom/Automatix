using OpenQA.Selenium.Chrome;

namespace Automatix.YTDownloader.Services.Selenium;

public class BrowserAdjustService
{

    public ChromeDriverService CreateDriverService()
    {
        var chromeDriverService = ChromeDriverService.CreateDefaultService();

        chromeDriverService.HideCommandPromptWindow = true;

        return chromeDriverService;
    }

    public ChromeOptions CreateChromeOptions(string appHttp, bool hideBrowser = true)
    {
        var options = new ChromeOptions();

        options.AcceptInsecureCertificates = true;

        if(hideBrowser)
            options.AddArgument("--window-position=-32000,-32000");

        //options.AddArgument("--headless");
        options.AddArgument("disable-gpu");
        options.AddArgument("--start-maximized");
        options.AddArgument("--enable-javascript");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument($"--app={appHttp}");
        options.AddArgument(
            "--user-agent='Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Mobile Safari/537.36'");
        options.AddArgument("ignore-certificate-errors");
        options.AddArgument("allow-insecure-localhost");
        options.AddArgument("mute-audio");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddExcludedArgument("enable-automation");
        options.AddArguments("--disable-notifications");
        options.AddArguments("--use-fake-ui-for-media-stream");
        options.AddArguments("--disable-user-media-security");
        options.AddArguments("--allow-running-insecure-content");
        //options.AddArguments("use-fake-device-for-media-stream");
        options.AddLocalStatePreference("useAutomationExtension", false);

        return options;
    }
}