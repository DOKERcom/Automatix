using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Automatix.YTDownloader.Services.Telegram;

public static class MessageSender
{
    public static async Task<Message> SendMessageAsync(ITelegramBotClient botClient, ChatId chatId, string text, CancellationToken cancellationToken)
    {
        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);

        return sentMessage;
    }

    public static async Task<Message> SendMessageAsync(ITelegramBotClient botClient, ChatId chatId, string text, ReplyKeyboardMarkup replyKeyboardMarkup, CancellationToken cancellationToken)
    {
        var sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: text,
            replyMarkup: replyKeyboardMarkup,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);

        return sentMessage;
    }
}