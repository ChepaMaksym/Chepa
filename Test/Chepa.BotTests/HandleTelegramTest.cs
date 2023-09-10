using Chepa.Bot.Db;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = Warehouse.Manager.User;
using Chepa.Bot;
using Microsoft.EntityFrameworkCore;

namespace Chepa.BotTests
{
    public class Tests
    {
        [Test]
        public async Task HandleTelegram_CreateUserIfNotExist_UserExistInDatabase()
        {
            var options = new DbContextOptionsBuilder<ChepaBotContext>()
                .Options;

            var botClientMock = new Mock<ITelegramBotClient>();

            using (var dbContext = new ChepaBotContext(options))
            {
                dbContext.Users.Add(new User("existing_user", 12345));
                dbContext.SaveChanges();
            }

            using (var dbContext = new ChepaBotContext(options))
            {
                var handle = new HandleTelegram(dbContext);

                var update = new Update
                {
                    Message = new Message
                    {
                        Chat = new Chat
                        {
                            Username = "existing_user"
                        }
                    }
                };

                var result = await handle.CreateUserIfNotExist(botClientMock.Object, update);

                Assert.AreEqual("existing_user", result.UserName);
            }

        }
    }
}