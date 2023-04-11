using Automatix.VideoWorker.Models;
using Automatix.VideoWorker.Services;
using System.IO;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Automatix.VideoWorker.Processors;

public class VideoProcessor
{
    private XNetService req = new();

    private readonly ITelegramBotClient _botClient;

    private readonly AppConfigModel _cfg;

    private readonly CancellationTokenSource _cancellationToken;

    private readonly ChangeAspectRatioProcessor _changeAspectRatioProcessor;

    private readonly SplitVideoProcessor _splitVideoProcessor;

    public VideoProcessor(string token, AppConfigModel cfg)
    {
        _botClient = new TelegramBotClient(token);

        _cfg = cfg;

        _cancellationToken = new CancellationTokenSource();

        _changeAspectRatioProcessor =
            new ChangeAspectRatioProcessor(_botClient, _cfg, _cancellationToken.Token);

        _splitVideoProcessor = new SplitVideoProcessor(_botClient, _cfg, _cancellationToken.Token);
    }

    public void StartProcessing()
    {
        var downloadedThread = new Thread(ProcessDownloadedFiles);
        downloadedThread.Start();

        var changedAspectRatioThread = new Thread(ProcessChangedAspectRatioFiles);
        changedAspectRatioThread.Start();
    }

    public async void ProcessDownloadedFiles()
    {
        while (true)
        {
            var files = Directory.GetFiles(_cfg.DownloadedVideoPath);
            if (files.Length > 0)
               await _changeAspectRatioProcessor.RunChangeAspectRatio(Path.GetFullPath(files[0]));

            Thread.Sleep(1000);
        }
    }

    public async void ProcessChangedAspectRatioFiles()
    {
        while (true)
        {
            var files = Directory.GetFiles(_cfg.ChangedAspectRatioVideoPath);

            if (files.Length > 0)
                await _splitVideoProcessor.RunSplitVideo(Path.GetFullPath(files[0]));

            Thread.Sleep(1000);
        }
    }
}