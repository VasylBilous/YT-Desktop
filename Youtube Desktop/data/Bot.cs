using System.IO;
using Telegram.Bot;

namespace Youtube_Desktop.data
{
    class Bot
    {
        static private readonly TelegramBotClient bot = new TelegramBotClient("1222021696:AAFRsRfmqvmHFHUYVgEkUOQMM23CJTDMKvo");
        static public async void SendAudio(string userId, string filePath)
        {
            long length = new System.IO.FileInfo(filePath).Length;

            if (length < 50000000)
            {
                using (FileStream fs = File.OpenRead(filePath))
                {
                    await bot.SendAudioAsync(chatId: userId, audio: fs, caption: Path.GetFileName(filePath));
                }
                File.Delete(filePath);
            }
            else
                await bot.SendTextMessageAsync(userId, "Hi from YT Desktope. Sorry but file size is too big :C");
        }
        static public async void SendVideo(string userId, string filePath)
        {
            long length = new System.IO.FileInfo(filePath).Length;

            if (length < 50000000)
            {
                using (FileStream fs = File.OpenRead(filePath))
                {
                    await bot.SendVideoAsync(chatId: userId, video: fs, caption: Path.GetFileName(filePath));
                }
                File.Delete(filePath);
            }
            else
                await bot.SendTextMessageAsync(userId, "Hi from YT Desktope. Sorry but file size is too big :C");
        }
    }
}
