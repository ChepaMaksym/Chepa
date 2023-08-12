using System;
using System.Threading;
using Telegram.Bot;
using Key;
namespace Chepa.Bot
{
    class Program
    {
        private static ITelegramBotClient botClient;
        static void Main()
        {

            botClient = new TelegramBotClient(PrivateKey.API_TOKEN);
            var me = botClient.GetMeAsync().Result;
            HandleTelegram handleTelegram = new HandleTelegram();
            botClient.StartReceiving(handleTelegram.HandleUpdatesAsync, handleTelegram.HandleError);
            Console.WriteLine(
              $"Hello, World! I am {me.FirstName} username {me.Username} and my id {me.Id}.");
            Thread.Sleep(int.MaxValue);
        }
    }
}