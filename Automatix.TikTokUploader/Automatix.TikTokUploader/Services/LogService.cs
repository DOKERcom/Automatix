using System.Text;
using Automatix.TikTokUploader.Services.Telegram;
using Telegram.Bot;

namespace Automatix.TikTokUploader.Services;

public class LogService
{
    public static void Log(string text)
    {
        using var fstream = new FileStream($"Log-TTUploader-{DateTime.Now:MM.dd.yy}.txt", FileMode.Append);

        var buffer = Encoding.Default.GetBytes(text + "\n");

        fstream.WriteAsync(buffer);

        fstream.Close();

        Console.WriteLine(text);
    }

    public static async Task Report(string text, ITelegramBotClient botClient, string chatId, CancellationToken cancellationToken)
    {
        Log(text);

        try
        {
            await MessageSender.SendMessageAsync(botClient, chatId, text, cancellationToken);
        }
        catch{}
    }
}