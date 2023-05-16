using Automatix.YTUploader.Models;
using Automatix.YTUploader.Services.Selenium;
using Automatix.YTUploader.Services;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Telegram.Bot;
using Automatix.YTUploader.Constants;

namespace Automatix.YTUploader.Processors;

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
        await LogService.Report($"{DateTime.Now} : Start processing of uploading service", _botClient,
            _cfg.ChatIdForReport,
            _cancellationToken.Token);

        var browserAdjustService = new BrowserAdjustService();

        bool hideBrowser = _cfg.HideBrowser == "1";

        var options = browserAdjustService.CreateChromeOptions("https://youtube.com/", hideBrowser);

        var chromeDriverService = browserAdjustService.CreateDriverService();

        IWebDriver driver = new ChromeDriver(chromeDriverService, options);

        var _bcs = new BrowserСontrolService(driver);

        try
        {
            driver.Navigate().GoToUrl("https://youtube.com/");

            if (hideBrowser)
                _bcs.HideWindow();

            var cookiefilePath = "cookies.json";

            // Чтение кук из файла JSON
            var cookiesArray = JArray.Parse(File.ReadAllText(cookiefilePath));
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

            if (_bcs.WaitForElementByXpath(YouTube.BtnCreateVideoXpath))
            {
                LogService.Log($"{DateTime.Now} : Login in YouTube detected!");
                while (true)
                {
                    var files = Directory.GetFiles(_cfg.UploadVideoFromPath);

                    if (files.Length > 0)
                        await UploadVideo(driver, _bcs, Path.GetFullPath(files[0]));

                    Thread.Sleep(Convert.ToInt32(_cfg.WaitingAfterUploadingVideoSeconds) * 1000);
                }
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
        await LogService.Report($"{DateTime.Now} : New video detected start uploading", _botClient, _cfg.ChatIdForReport,
            _cancellationToken.Token);

        if (_bcs.WaitForElementByXpath(YouTube.BtnCreateVideoXpath, 10))
        {

            _bcs.ClickOnElementByXpath(YouTube.BtnCreateVideoXpath);

            _bcs.ClickOnElementByXpath(YouTube.BtnAddVideoXpath);

            var elemInput = _bcs.FindElementByXpath(YouTube.InputUploadVideoXpath);

            elemInput.SendKeys(filePath);

            if (_bcs.WaitForElementByXpath(YouTube.InputVideoNameXpath, 10))
            {
                var capture = GenerateCapture(_cfg.Captions, _cfg.Tags, Convert.ToInt32(_cfg.NumberGenerationTags));

                _bcs.RewriteToInputByXpath(YouTube.InputVideoNameXpath, capture);

                _bcs.ClickOnElementByXpath(YouTube.BtnNextXpath);

                Thread.Sleep(500);

                _bcs.ClickOnElementByXpath(YouTube.BtnNextXpath);

                Thread.Sleep(500);

                _bcs.ClickOnElementByXpath(YouTube.BtnNextXpath);

                Thread.Sleep(60000);

                _bcs.ClickOnElementByXpath(YouTube.BtnDoneXpath);

                if (_bcs.WaitForElementByXpath(YouTube.LinkToVideoXpath, 10))
                {
                    driver.Navigate().GoToUrl("https://youtube.com/");

                    MoveFiles(filePath, _cfg.ArchiveVideoPath);

                    await LogService.Report($"{DateTime.Now} : Successfully upload video file {filePath}!",
                        _botClient, _cfg.ChatIdForReport, _cancellationToken.Token);
                }
                else
                    await LogService.Report($"{DateTime.Now} : Element YouTube.BtnCreateVideoXpath not found.", _botClient,
                        _cfg.ChatIdForReport, _cancellationToken.Token);

            }
            else
                await LogService.Report($"{DateTime.Now} : Element YouTube.BtnCreateVideoXpath not found.", _botClient,
                    _cfg.ChatIdForReport, _cancellationToken.Token);
        }
        else
            await LogService.Report($"{DateTime.Now} : Element YouTube.BtnCreateVideoXpath not found.", _botClient,
                _cfg.ChatIdForReport, _cancellationToken.Token);
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