using Automatix.VideoConverter.Constants;
using Automatix.VideoConverter.Models;
using Automatix.VideoConverter.Services;
using Automatix.VideoConverter.Services.Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using Telegram.Bot;

namespace Automatix.VideoConverter.Processors;

public class ConvertProcessor
{
    private readonly XNetService req = new();

    private readonly ITelegramBotClient _botClient;

    private readonly AppConfigModel _cfg;

    private readonly CancellationToken _cancellationToken;

    public ConvertProcessor(ITelegramBotClient botClient, AppConfigModel cfg, CancellationToken cancellationToken)
    {
        _botClient = botClient;

        _cfg = cfg;

        _cancellationToken = cancellationToken;
    }

    public async Task RunConverting(string filePath)
    {
        await LogService.Report($"{DateTime.Now} : Start converting video: {filePath}", _botClient, _cfg.ChatIdForReport,
            _cancellationToken);

        var browserAdjustService = new BrowserAdjustService();

        bool hideBrowser = _cfg.HideBrowser == "1";

        var options = browserAdjustService.CreateChromeOptions("https://video.online-convert.com/convert-to-mp4", hideBrowser, "");

        var chromeDriverService = browserAdjustService.CreateDriverService();

        IWebDriver driver = new ChromeDriver(chromeDriverService, options);

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

        var js = (IJavaScriptExecutor)driver;

        var _bcs = new BrowserСontrolService(driver);

        if (hideBrowser)
            _bcs.HideWindow();

        try
        {
            if (_bcs.WaitForElementById(VideoOnlineConvert.InputBrowseFileId))
            {
                var elemInput = _bcs.FindElementById(VideoOnlineConvert.InputBrowseFileId);

                elemInput.SendKeys(filePath);

                Thread.Sleep(3000);

                if (_bcs.WaitUntilElementExistsByXpath(VideoOnlineConvert.SpanFileLoadingXpath, 240))
                {
                    _bcs.ClickOnElementByXpath(VideoOnlineConvert.ButtonRunConvertXpath);

                    if (_bcs.WaitForElementByXpath(VideoOnlineConvert.HrefDownloadXpath, 240))
                    {
                        var url = _bcs.GetElementAttributeByXpath(VideoOnlineConvert.HrefDownloadXpath, "href");

                        driver.Quit();

                        var fileName = Path.GetRandomFileName().Replace(".", "") + ".mp4";

                        File.Delete(filePath);

                        var thread = new Thread(() =>
                        {
                            SaveFile(url, _cfg.ConvertedVideoPath + fileName);
                        });

                        thread.Start();
                    }
                }
            }
        }
        catch (Exception e)
        {
            await LogService.Report($"{DateTime.Now} : ERROR in driver during converting: {e.Message}", _botClient, _cfg.ChatIdForReport, _cancellationToken);
            driver.Quit();
        }
    }
    private async void SaveFile(string link, string path)
    {
        try
        {
            var response = req.DownloadFile(link);

            await File.WriteAllBytesAsync(path, response.ToArray(), _cancellationToken);

            await LogService.Report($"{DateTime.Now} : Success: file {path} converted, downloaded & saved!", _botClient, _cfg.ChatIdForReport, _cancellationToken);
        }
        catch (Exception e)
        {
            await LogService.Report($"{DateTime.Now} : ERROR during converting: {e.Message}", _botClient, _cfg.ChatIdForReport, _cancellationToken);
        }
    }
}