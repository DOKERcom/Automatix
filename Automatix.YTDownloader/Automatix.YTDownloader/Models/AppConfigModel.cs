namespace Automatix.YTDownloader.Models;

public class AppConfigModel
{
    public string? TelegramBotToken { get; set; }

    public string[]? AcceptableChatIds { get; set; }

    public string? DownloadedVideoPath { get; set; }

    public string? ChatIdForReport { get; set; }

    public string? HideBrowser { get; set; }
}