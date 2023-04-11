using Automatix.VideoWorker.Models;
using Automatix.VideoWorker.Services.Selenium;
using Automatix.VideoWorker.Services;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Telegram.Bot;
using Automatix.VideoWorker.Constants;
using File = System.IO.File;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;

namespace Automatix.VideoWorker.Processors;

public class SplitVideoProcessor
{
    private XNetService req = new();

    private readonly ITelegramBotClient _botClient;

    private readonly AppConfigModel _cfg;

    private readonly CancellationToken _cancellationToken;

    public SplitVideoProcessor(ITelegramBotClient botClient, AppConfigModel cfg, CancellationToken cancellationToken)
    {
        _botClient = botClient;

        _cfg = cfg;

        _cancellationToken = cancellationToken;
    }

    public async Task RunSplitVideo(string filePath)
    {
        await LogService.Report($"{DateTime.Now} : Start splitting of video: {filePath}", _botClient, _cfg.ChatIdForReport,
            _cancellationToken);

        var browserAdjustService = new BrowserAdjustService();

        bool hideBrowser = _cfg.HideBrowser == "1";

        var options = browserAdjustService.CreateChromeOptions("https://split-video.com/", hideBrowser,
            Path.Combine(Directory.GetCurrentDirectory(), _cfg.BrowserDownloadPath.Replace("/", "\\")));

        var chromeDriverService = browserAdjustService.CreateDriverService();

        IWebDriver driver = new ChromeDriver(chromeDriverService, options);

        var js = (IJavaScriptExecutor)driver;

        var _bcs = new BrowserСontrolService(driver);

        if(hideBrowser)
            _bcs.HideWindow();

        try
        {
            if (_bcs.WaitForElementByXpath(ResizeVideo.InputBrowseFileXpath))
            {
                var elemInput = _bcs.FindElementByXpath(SplitVideo.InputBrowseFileXpath);

                elemInput.SendKeys(filePath);

                Thread.Sleep(1000);

                var scrollHeight = Convert.ToInt32(js.ExecuteScript("return document.documentElement.scrollHeight"));
                var scrollTo = Convert.ToInt32(scrollHeight * 0.7);
                js.ExecuteScript($"window.scrollBy(0, {scrollTo});");

                _bcs.DeleteElementByXpath(SplitVideo.ButtonCloseCookieXpath);

                _bcs.ClickOnElementByXpath(SplitVideo.SecondTabXpath);

                _bcs.RewriteToInputById(SplitVideo.SplitNumberId, GetCountSplitParts(filePath).ToString());

                _bcs.ClickOnElementById(SplitVideo.ButtonSplitId);

                var list =_bcs.FindElementsByXpath(SplitVideo.TableSplitItemsXpath);

                var listLinks = new List<string>();

                while (list.Count > 0)
                {
                    foreach(var item in list)
                    {
                        try
                        {
                            var aElement = item.FindElement(By.TagName("a"));

                            var checkAttr = aElement.GetAttribute("download");

                            if (!string.IsNullOrEmpty(checkAttr))
                            {
                                var url = aElement.GetAttribute("href");

                                if (!string.IsNullOrEmpty(url) && !listLinks.Contains(url))
                                {
                                    aElement.Click();

                                    listLinks.Add(url);
                                }
                            }
                        }
                        catch { }
                    }

                    list = _bcs.FindElementsByXpath(SplitVideo.TableSplitItemsXpath);

                    if (list.Count == listLinks.Count)
                    {
                        Thread.Sleep(10000);

                        MoveFiles(_cfg.BrowserDownloadPath, _cfg.CuttedVideoPath);

                        File.Delete(filePath);

                        await LogService.Report($"{DateTime.Now} : Success: files {_cfg.CuttedVideoPath} splitted, downloaded & saved!", _botClient, _cfg.ChatIdForReport, _cancellationToken);
                        break;
                    }

                    Thread.Sleep(5000);
                }
            }
            else
                await LogService.Report($"{DateTime.Now} : Element {ResizeVideo.InputBrowseFileXpath} not found.", _botClient,
                    _cfg.ChatIdForReport, _cancellationToken);
        }
        catch (Exception e)
        {
            await LogService.Report($"{DateTime.Now} : ERROR in driver during splitting: {e.Message}", _botClient, _cfg.ChatIdForReport, _cancellationToken);
            driver.Quit();
        }

        driver.Quit();
    }


    private void MoveFiles(string downloadPath, string cuttedVideoPath)
    {
        var files = Directory.GetFiles(downloadPath);

        foreach (var file in files)
        {
            var fileName = Path.GetRandomFileName().Replace(".", "") + ".mp4";

            var destinationPath = Path.Combine(cuttedVideoPath, fileName);

            File.Move(file, destinationPath);
        }
    }

    private async void SaveFile(IWebDriver driver, string link, string path)
    {
        try
        {
            var js = (IJavaScriptExecutor)driver;

            js.ExecuteScript("window.open('" + link + "', '_blank');");

            driver.SwitchTo().Window(driver.WindowHandles.Last());

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            var fileContent = ((ITakesScreenshot)driver).GetScreenshot().AsByteArray;

            File.WriteAllBytes(path, fileContent);


            await LogService.Report($"{DateTime.Now} : Success: file {path} splitted, downloaded & saved!", _botClient, _cfg.ChatIdForReport, _cancellationToken);
        }
        catch (Exception e)
        {
            await LogService.Report($"{DateTime.Now} : ERROR during spliting video file {path}: {e.Message}", _botClient, _cfg.ChatIdForReport, _cancellationToken);
        }
    }

    private int GetCountSplitParts(string filePath)
    {
        var shellType = Type.GetTypeFromProgID("Shell.Application");

        dynamic shell = Activator.CreateInstance(shellType);

        var folder = shell.NameSpace(Path.GetDirectoryName(filePath));

        var folderItem = folder.ParseName(Path.GetFileName(filePath));

        var lengthStr = folder.GetDetailsOf(folderItem, 27);

        TimeSpan duration = TimeSpan.Parse(lengthStr);

        var dur = duration.TotalSeconds;

        var length = dur / 60.0;

        return (int)Math.Ceiling(length)*2;
    }
}