using Automatix.VideoWorker.Models;
using Automatix.VideoWorker.Services;
using Automatix.VideoWorker.Services.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using Automatix.VideoWorker.Constants;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;
using OpenQA.Selenium.Support.UI;

namespace Automatix.VideoWorker.Processors;

public class ChangeAspectRatioProcessor
{
    private readonly XNetService req = new();

    private readonly ITelegramBotClient _botClient;

    private readonly AppConfigModel _cfg;

    private readonly CancellationToken _cancellationToken;

    public ChangeAspectRatioProcessor(ITelegramBotClient botClient, AppConfigModel cfg, CancellationToken cancellationToken)
    {
        _botClient = botClient;

        _cfg = cfg;

        _cancellationToken = cancellationToken;
    }

    public async Task RunChangeAspectRatio(string filePath)
    {
        await LogService.Report($"{DateTime.Now} : Start change aspect ratio of video: {filePath}", _botClient, _cfg.ChatIdForReport,
            _cancellationToken);

        var browserAdjustService = new BrowserAdjustService();

        bool hideBrowser = _cfg.HideBrowser == "1";

        var options = browserAdjustService.CreateChromeOptions("https://www.resize-video.com/", hideBrowser);

        var chromeDriverService = browserAdjustService.CreateDriverService();

        IWebDriver driver = new ChromeDriver(chromeDriverService, options);

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

        var js = (IJavaScriptExecutor)driver;

        var _bcs = new BrowserСontrolService(driver);

        if(hideBrowser)
            _bcs.HideWindow();

        try
        {
            if (_bcs.WaitForElementByXpath(ResizeVideo.InputBrowseFileXpath))
            {
                var elemInput = _bcs.FindElementByXpath(ResizeVideo.InputBrowseFileXpath);
                elemInput.SendKeys(filePath);

                if (_bcs.WaitForElementById(ResizeVideo.SelectId))
                {
                    _bcs.SetOptionInSelectByIdByValue(ResizeVideo.SelectId, "resize-crop");

                    _bcs.RewriteToInputById(ResizeVideo.InputHeightId, "1280");

                    _bcs.RewriteToInputById(ResizeVideo.InputWidthId, "720");

                    //while(_bcs.DeleteElementByXpath("//ins[@class=\"adsbygoogle\"]")){}

                    _bcs.ClickOnElementByXpath(ResizeVideo.ButtonSubmitXpath);

                    if (_bcs.WaitForElementByXpath(ResizeVideo.LinkToFileXpath, 240))
                    {
                        var url = _bcs.GetElementAttributeByXpath(ResizeVideo.LinkToFileXpath, "href");

                        var fileName = Path.GetRandomFileName().Replace(".", "") + ".mp4";

                        File.Delete(filePath);

                        var thread = new Thread(() =>
                        {
                            SaveFile(url, _cfg.ChangedAspectRatioVideoPath + fileName);
                        });

                        thread.Start();
                    }
                    else
                        await LogService.Report($"{DateTime.Now} : Element {ResizeVideo.LinkToFileXpath} not found.", _botClient,
                            _cfg.ChatIdForReport, _cancellationToken);
                }
                else
                    await LogService.Report($"{DateTime.Now} : Element {ResizeVideo.SelectId} not found.", _botClient,
                        _cfg.ChatIdForReport, _cancellationToken);
            }
            
        }
        catch (Exception e)
        {
            await LogService.Report($"{DateTime.Now} : ERROR in driver during changing aspect: {e.Message}", _botClient, _cfg.ChatIdForReport, _cancellationToken);
            driver.Quit();
        }
        driver.Quit();
    }

    private async void SaveFile(string link, string path)
    {
        try
        {
            var response = req.DownloadFile(link);

            await File.WriteAllBytesAsync(path, response.ToArray(), _cancellationToken);

            await LogService.Report($"{DateTime.Now} : Success: file {path} changed aspect ratio, downloaded & saved!", _botClient, _cfg.ChatIdForReport, _cancellationToken);
        }
        catch (Exception e)
        {
            await LogService.Report($"{DateTime.Now} : ERROR during changing aspect ratio: {e.Message}", _botClient, _cfg.ChatIdForReport, _cancellationToken);
        }
    }
}