using Automatix.YTUploader.Models;
using Automatix.YTUploader.Services.Selenium;
using Automatix.YTUploader.Services;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Telegram.Bot;

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

        var options = browserAdjustService.CreateChromeOptions(hideBrowser);

        var chromeDriverService = browserAdjustService.CreateDriverService();

        IWebDriver driver = new ChromeDriver(chromeDriverService, options);

        var _bcs = new BrowserСontrolService(driver);

        try
        {
            driver.Navigate().GoToUrl("https://youtube.com/");

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

                Thread.Sleep(3000);
        }
        catch (Exception e)
        {
            await LogService.Report($"{DateTime.Now} : ERROR during uploading videos: {e.Message}", _botClient,
                _cfg.ChatIdForReport, _cancellationToken.Token);
            driver.Quit();
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