using Automatix.YTDownloader.Helpers;
using Automatix.YTDownloader.Models;
using Automatix.YTDownloader.Services.Telegram;

namespace Automatix.YTDownloader;

public class Startup
{
    public void Run(AppConfigModel cfg)
    {
        if(!TryCreateDownloadedVideoFolder(cfg))
            return;

        MessageReceiver messageReceiver = new(cfg.TelegramBotToken ??
                                                            throw new InvalidOperationException(
                                                                "Startup: TelegramBotToken can't be null or empty"), cfg);
        messageReceiver.StartReceiving();
    }

    private bool TryCreateDownloadedVideoFolder(AppConfigModel cfg)
    {
        try
        {
            if (cfg.DownloadedVideoPath == null)
                return false;

            FolderHelper.CreateFolder(cfg.DownloadedVideoPath);

            return true;
        }
        catch
        {
            return false;
        }
    }
}