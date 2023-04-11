using Automatix.YTDownloader.Models;
using Automatix.YTDownloader.Processors;
using Telegram.Bot;

namespace Automatix.YTDownloader.Services.Telegram;

public class MessageHandler
{
    public async Task Handle(ITelegramBotClient botClient, AppConfigModel cfg, string message, long id, CancellationToken cancellationToken)
    {
        if (!message.StartsWith("http"))
        {
            await MessageSender.SendMessageAsync(botClient, cfg.ChatIdForReport,
                "ERROR Automatix.YTDownloader.MessageHandler.Handle: invalid http message.", cancellationToken);

            return;
        }

        VideoDownloadProcessor _videoDownloadProcessor = new(botClient, cfg, cancellationToken);

        var thread = new Thread(() => { _videoDownloadProcessor.DownloadVideo(message); });

        thread.Start();
    }
}