using Automatix.YTDownloader.Services;
using Automatix.YTDownloader.Services.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Text.RegularExpressions;
using Automatix.YTDownloader.Constants;
using Automatix.YTDownloader.Models;
using Telegram.Bot;
using OpenQA.Selenium.Support.UI;

namespace Automatix.YTDownloader.Processors;

public class VideoDownloadProcessor
{
    private XNetService req = new();

    private readonly ITelegramBotClient _botClient;

    private readonly AppConfigModel _cfg;

    private CancellationToken _cancellationToken;

    public VideoDownloadProcessor(ITelegramBotClient botClient, AppConfigModel cfg, CancellationToken cancellationToken)
    {
        _botClient = botClient;

        _cfg = cfg;

        _cancellationToken = cancellationToken;
    }

    public async void DownloadVideo(string url)
    {
        await LogService.Report($"{DateTime.Now} : Start download video: {url}", _botClient, _cfg.ChatIdForReport,
            _cancellationToken);

        var browserAdjustService = new BrowserAdjustService();

        bool hideBrowser = _cfg.HideBrowser == "1";

        var newurl = CreateDownloadUrl(url);
        
        var options = browserAdjustService.CreateChromeOptions(newurl, hideBrowser);

        var chromeDriverService = browserAdjustService.CreateDriverService();

        IWebDriver driver = new ChromeDriver(chromeDriverService, options);

        var js = (IJavaScriptExecutor)driver;

        var _bcs = new BrowserСontrolService(driver);

        if(hideBrowser)
            _bcs.HideWindow();

        try
        {
            if (_bcs.WaitForElementByXpath(Y2mate.ButtonDownloadXpath, 30))
            {

                var onClickValue = _bcs.GetElementAttributeByXpath(Y2mate.ButtonDownloadXpath, "onclick");

                js.ExecuteScript(onClickValue);

                if (_bcs.WaitForElementByXpath(Y2mate.HrefDownloadXpath, 30))
                {
                    var link = _bcs.GetElementAttributeByXpath(Y2mate.HrefDownloadXpath, "href");

                    var fileName = Path.GetRandomFileName().Replace(".", "") + ".mp4";

                    var thread = new Thread(() =>
                    {
                        SaveFile(link, _cfg.DownloadedVideoPath + fileName);
                    });

                    thread.Start();
                }
                else
                    await LogService.Report($"{DateTime.Now} : Element {Y2mate.HrefDownloadXpath} not found.",
                        _botClient, _cfg.ChatIdForReport, _cancellationToken);
            }
            else
                await LogService.Report($"{DateTime.Now} : Element {Y2mate.ButtonDownloadXpath} not found.", _botClient,
                    _cfg.ChatIdForReport, _cancellationToken);

        }
        catch (Exception e)
        {
            await LogService.Report($"{DateTime.Now} : ERROR in driver during downloading: {e}", _botClient, _cfg.ChatIdForReport, _cancellationToken);
        }

        driver.Quit();
    }

    private async void SaveFile(string link, string path)
    {
        try
        {
            var response = req.DownloadFile(link);

            await File.WriteAllBytesAsync(path, response.ToArray(), _cancellationToken);

            await LogService.Report($"{DateTime.Now} : Success: file {path} downloaded!", _botClient, _cfg.ChatIdForReport, _cancellationToken);
        }
        catch (Exception e)
        {
            await LogService.Report($"{DateTime.Now} : ERROR during downloading or saving: {e}", _botClient, _cfg.ChatIdForReport, _cancellationToken);
        }
    }

    private string CreateDownloadUrl(string input)
    {
        var pattern = @"watch\?v=([a-zA-Z0-9_-]{11})";

        var match = Regex.Match(input, pattern);

        var ownUrl = "https://www.y2mate.com/youtube/";

        if (match.Success)
        {
            var videoId = match.Groups[1].Value;

            ownUrl += videoId;

            return ownUrl;
        }

        return null;
    }
}