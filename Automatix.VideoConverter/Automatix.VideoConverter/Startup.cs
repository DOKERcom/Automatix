using Automatix.VideoConverter.Helpers;
using Automatix.VideoConverter.Models;
using Automatix.VideoConverter.Processors;

namespace Automatix.VideoConverter;

public class Startup
{
    public void Run(AppConfigModel cfg)
    {
        if (!TryCreateFolder(cfg.CuttedVideoPath))
            return;

        if (!TryCreateFolder(cfg.ConvertedVideoPath))
            return;

        if (!TryCreateFolder(cfg.UploadVideoFromPath))
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