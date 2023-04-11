using Automatix.YTDownloader.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Automatix.YTDownloader.Services.Telegram;

public class MessageReceiver
{
    private readonly ITelegramBotClient _botClient;

    private readonly CancellationTokenSource _cancellationToken;

    private readonly MessageHandler _messageHandler;

    private readonly AppConfigModel _cfg;

    public MessageReceiver(string token, AppConfigModel cfg)
    {
        _botClient = new TelegramBotClient(token);

        _cancellationToken = new CancellationTokenSource();

        _messageHandler = new MessageHandler();

        _cfg = cfg;
    }

    public void StartReceiving()
    {
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: _cancellationToken.Token
        );
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;

        if (message.Text is not { } messageText)
            return;

        Console.WriteLine($"MessageReceiver: Received '{messageText}' message in chat {message.Chat.Id}.");

        if (_cfg.AcceptableChatIds == null)
            return;

        if (!_cfg.AcceptableChatIds.Contains(message.Chat.Id.ToString()))
            return;

        if(string.IsNullOrEmpty(_cfg.ChatIdForReport))
            _cfg.ChatIdForReport = message.Chat.Id.ToString();

        _messageHandler.Handle(botClient, _cfg, messageText, message.Chat.Id, cancellationToken);
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}