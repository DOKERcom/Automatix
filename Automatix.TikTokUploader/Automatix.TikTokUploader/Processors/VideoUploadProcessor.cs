using Automatix.TikTokUploader.Models;
using Automatix.TikTokUploader.Services;
using Automatix.TikTokUploader.Services.Selenium;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using Automatix.TikTokUploader.Constants;
using Telegram.Bot;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Interactions;
using System.IO;
using System.Text.RegularExpressions;

namespace Automatix.TikTokUploader.Processors;

public class VideoUploadProcessor
{
    private readonly ITelegramBotClient _botClient;

    private readonly AppConfigModel _cfg;

    private readonly CancellationTokenSource _cancellationToken;


    public VideoUploadProcessor(string token, AppConfigModel cfg)
    {
        _botClient = new TelegramBotClient(token);

        _cfg = cfg;

        _cancellationToken = new CancellationTokenSource();
    }

    public async void StartProcessing()
    {
        await LogService.Report($"{DateTime.Now} : Start processing of uploading service", _botClient, _cfg.ChatIdForReport,
            _cancellationToken.Token);

        var browserAdjustService = new BrowserAdjustService();

        bool hideBrowser = _cfg.HideBrowser == "1";

        var options = browserAdjustService.CreateChromeOptions(hideBrowser);

        var chromeDriverService = browserAdjustService.CreateDriverService();

        IWebDriver driver = new ChromeDriver(chromeDriverService, options);

        var _bcs = new BrowserСontrolService(driver);

        try
        {
            driver.Navigate().GoToUrl("https://www.tiktok.com/upload");

            if (!_bcs.WaitForElementByXpath(Tiktok.UploadFrameXpath, 10))
            {
                var filePath = "cookies.json";

                // Чтение кук из файла JSON
                var cookiesArray = JArray.Parse(File.ReadAllText(filePath));
                foreach (JObject cookie in cookiesArray)
                {
                    string name = cookie["name"].ToString();
                    string value = cookie["value"].ToString();
                    string domain = cookie["domain"].ToString();
                    string path = cookie["path"].ToString();
                    string expiry = "1713442609";
                    try
                    {
                        expiry = cookie["expirationDate"].ToString();
                    }
                    catch
                    {
                    }

                    int dotIndex = expiry.IndexOf('.');
                    if (dotIndex != -1)
                    {
                        expiry = expiry.Substring(0, dotIndex);
                    }

                    var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                        .AddSeconds(Convert.ToInt64(expiry)).ToLocalTime();
                    // Создание объекта Cookie
                    Cookie seleniumCookie = new Cookie(name, value, domain, path, dateTime);

                    // Добавление куки в драйвер
                    driver.Manage().Cookies.AddCookie(seleniumCookie);
                }

                driver.Navigate().Refresh();
            }

            Thread.Sleep(3000);

            if (_bcs.WaitForElementByXpath(Tiktok.UploadFrameXpath, 30))
            {
                _bcs.SwitchToFrameByXpath(Tiktok.UploadFrameXpath);

                if (_bcs.WaitForElementByXpath(Tiktok.InputFileXapth))
                {
                    LogService.Log($"{DateTime.Now} : Login in tiktok detected!");

                    if (hideBrowser)
                        _bcs.HideWindow();

                    while (true)
                    {
                        _bcs.SwitchToFrameByXpath(Tiktok.UploadFrameXpath);

                        var files = Directory.GetFiles(_cfg.UploadVideoFromPath);

                        if (files.Length > 0)
                            await UploadVideo(driver, _bcs, Path.GetFullPath(files[0]));

                        Thread.Sleep(Convert.ToInt32(_cfg.WaitingAfterUploadingVideoSeconds) * 1000);
                    }
                }
            }
            else
            {
                driver.Quit();
                await LogService.Report(
                    $"{DateTime.Now} : You have not logged in to your TikTok account within 30 seconds, the service is stopped.",
                    _botClient, _cfg.ChatIdForReport,
                    _cancellationToken.Token);
            }
        }
        catch (Exception e)
        {
            await LogService.Report($"{DateTime.Now} : ERROR during uploading videos: {e.Message}", _botClient,
                _cfg.ChatIdForReport, _cancellationToken.Token);
            driver.Quit();
        }
    }

    private async Task UploadVideo(IWebDriver driver, BrowserСontrolService _bcs, string filePath)
    {
        await LogService.Report($"{DateTime.Now} : New videos detected start uploading", _botClient, _cfg.ChatIdForReport,
            _cancellationToken.Token);
        
        try
        {
            if (_bcs.WaitForElementByXpath(Tiktok.InputFileXapth))
            {
                var elemInput = _bcs.FindElementByXpath(Tiktok.InputFileXapth);

                elemInput.SendKeys(filePath);

                Thread.Sleep(1000);

                _bcs.SwitchToFrameByXpath(Tiktok.UploadFrameXpath);

                if (_bcs.WaitWhenElementEnableByXpath(Tiktok.ButtonPostXpath, 240))
                {
                    Thread.Sleep(1000);

                    _bcs.SwitchToFrameByXpath(Tiktok.UploadFrameXpath);

                    _bcs.ClickOnElementByXpath(Tiktok.ImgBackgroundXpath);

                    var capture = GenerateCapture(_cfg.Captions, _cfg.Tags, Convert.ToInt32(_cfg.NumberGenerationTags));

                    var elem = driver.FindElement(By.XPath(Tiktok.EditCaptureXpath));

                    elem.Click();

                    int i = 0;
                    while (i < 15)
                    {
                        elem.SendKeys(Keys.Backspace);
                        i++;
                    }

                    if (_cfg.GenerateCapture == "1")
                    {
                        elem.SendKeys(capture);
                    }

                    _bcs.ClickOnElementByXpath(Tiktok.ButtonPostXpath);

                    Thread.Sleep(5000);

                    driver.Navigate().GoToUrl("https://www.tiktok.com/upload");

                    MoveFiles(filePath, _cfg.ArchiveVideoPath);

                    await LogService.Report($"{DateTime.Now} : Successfully upload viode file {filePath}!",
                        _botClient, _cfg.ChatIdForReport, _cancellationToken.Token);
                }
            }

        }
        catch (Exception e)
        {
            await LogService.Report($"{DateTime.Now} : ERROR during uploading video file {filePath}: {e.Message}",
                _botClient, _cfg.ChatIdForReport, _cancellationToken.Token);
        }
    }

    private string GenerateCapture(string[] captions, string[] tags, int numberTags)
    {
        var rnd = new Random();

        var caption = captions[rnd.Next(captions.Length)];

        var selectedTags = tags.OrderBy(x => rnd.Next()).Take(numberTags).ToArray();

        var tagsString = string.Join(" ", selectedTags);

        return $"{caption} {tagsString}";
    }

    private void MoveFiles(string cuttedVideoPath, string archiveVideoPath)
    {
        File.Move(cuttedVideoPath, archiveVideoPath+ Path.GetFileName(cuttedVideoPath));
    }
}