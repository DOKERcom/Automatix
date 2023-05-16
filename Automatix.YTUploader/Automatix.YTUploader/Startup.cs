﻿using Automatix.YTUploader.Helpers;
using Automatix.YTUploader.Models;
using Automatix.YTUploader.Processors;

namespace Automatix.YTUploader;

public class Startup
{
    public void Run(AppConfigModel cfg)
    {
        if (!TryCreateFolder(cfg.CuttedVideoPath))
            return;

        if (!TryCreateFolder(cfg.ArchiveVideoPath))
            return;

        if (!TryCreateFolder(cfg.ConvertedVideoPath))
            return;
        
        var videoProcessor = new VideoUploadProcessor(cfg.TelegramBotToken ??
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