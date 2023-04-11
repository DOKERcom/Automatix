namespace Automatix.TikTokUploader.Models;

public class AppConfigModel
{
    public string? TelegramBotToken { get; set; }

    public string? CuttedVideoPath { get; set; }

    public string? UploadVideoFromPath { get; set; }

    public string? ChatIdForReport { get; set; }

    public string? ArchiveVideoPath { get; set; }

    public string? WaitingAfterUploadingVideoSeconds { get; set; }

    public string? NumberGenerationTags { get; set; }

    public string[]? Captions { get; set; }

    public string[]? Tags { get; set; }

    public string? HideBrowser { get; set; }

    public string? GenerateCapture { get; set; }
}