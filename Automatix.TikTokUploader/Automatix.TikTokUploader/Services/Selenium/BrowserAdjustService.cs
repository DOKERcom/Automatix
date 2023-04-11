using OpenQA.Selenium.Chrome;

namespace Automatix.TikTokUploader.Services.Selenium;

public class BrowserAdjustService
{

    public ChromeDriverService CreateDriverService()
    {
        var chromeDriverService = ChromeDriverService.CreateDefaultService();

        chromeDriverService.HideCommandPromptWindow = true;

        return chromeDriverService;
    }

    public ChromeOptions CreateChromeOptions(bool hideBrowser = true, string defaultDownloadPath = @"C:\Downloads")
    {
        var options = new ChromeOptions();

        options.AddUserProfilePreference("download.default_directory", defaultDownloadPath);
        options.AddUserProfilePreference("download.prompt_for_download", false);
        options.AddUserProfilePreference("disable-popup-blocking", "true");
        options.AddUserProfilePreference("safebrowsing.enabled", "true");
        options.AddUserProfilePreference("plugins.plugins_disabled", new[] { "Chrome PDF Viewer" });
        options.AddUserProfilePreference("profile.default_content_settings.popups", 0);
        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("profile.password_manager_enabled", false);
        options.AcceptInsecureCertificates = true;

        if (hideBrowser)
            options.AddArgument("--window-position=-32000,-32000");

        //options.AddArgument("--headless");
        options.AddArgument("disable-gpu");
        //options.AddArgument("user-data-dir=C:\\Users\\Asus\\AppData\\Local\\Google\\Chrome\\User Data\\");
        //options.AddArgument("profile-directory=Default");
        options.AddArgument("--disable-extensions");
        //options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--start-maximized");
        options.AddArgument("--enable-javascript");
       // options.AddArgument("--disable-javascript");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        //options.AddArgument($"--app={url}");
        options.AddArgument(
            "--user-agent='Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36'");
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