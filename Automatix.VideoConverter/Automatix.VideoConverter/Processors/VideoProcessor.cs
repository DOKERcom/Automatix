using Automatix.VideoConverter.Models;
using Automatix.VideoConverter.Services;
using Telegram.Bot;

namespace Automatix.VideoConverter.Processors;

public class VideoProcessor
{
    private readonly XNetService req = new();

    private readonly ITelegramBotClient _botClient;

    private readonly AppConfigModel _cfg;

    private readonly CancellationTokenSource _cancellationToken;

    private readonly ConvertProcessor _convertProcessor;


    public VideoProcessor(string token, AppConfigModel cfg)
    {
        _botClient = new TelegramBotClient(token);

        _cfg = cfg;

        _cancellationToken = new CancellationTokenSource();

        _convertProcessor =
            new ConvertProcessor(_botClient, _cfg, _cancellationToken.Token);
    }

    public void StartProcessing()
    {
        var downloadedThread = new Thread(ConvertCuttedFiles);
        downloadedThread.Start();
    }

    public async void ConvertCuttedFiles()
    {
        while (true)
        {
            var files = Directory.GetFiles(_cfg.CuttedVideoPath);
            if (files.Length > 0)
                await _convertProcessor.RunConverting(Path.GetFullPath(files[0]));

            Thread.Sleep(1000);
        }
    }
}