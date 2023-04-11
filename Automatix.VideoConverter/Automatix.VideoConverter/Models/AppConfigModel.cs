namespace Automatix.VideoConverter.Models;

public class AppConfigModel
{
    public string? TelegramBotToken { get; set; }

    public string[]? AcceptableChatIds { get; set; }

    public string? CuttedVideoPath { get; set; }

    public string? UploadVideoFromPath { get; set; }

    public string? ConvertedVideoPath { get; set; }

    public string? BrowserDownloadPath { get; set; }

    public string? ChatIdForReport { get; set; }

    public string? HideBrowser { get; set; }
}