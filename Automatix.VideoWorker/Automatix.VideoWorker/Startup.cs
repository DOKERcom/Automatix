using Automatix.VideoWorker.Helpers;
using Automatix.VideoWorker.Models;
using Automatix.VideoWorker.Processors;
using Telegram.Bot;

namespace Automatix.VideoWorker;

public class Startup
{
    public void Run(AppConfigModel cfg)
    {
        if (!TryCreateFolder(cfg.DownloadedVideoPath))
            return;

        if (!TryCreateFolder(cfg.ChangedAspectRatioVideoPath))
            return;

        if (!TryCreateFolder(cfg.CuttedVideoPath))
            return;

        if (!TryCreateFolder(cfg.BrowserDownloadPath))
            return;

        var videoProcessor = new VideoProcessor(cfg.TelegramBotToken ??
                                                throw new InvalidOperationException(
                                                    "Startup: TelegramBotToken can't be null or empty"), cfg);

        videoProcessor.StartProcessing();
    }

    private bool TryCreateFolder(string path)
    {
        try
        {
            if (path == null)
                return false;

            FolderHelper.CreateFolder(path);

            return true;
        }
        catch
        {
            return false;
        }
    }
}