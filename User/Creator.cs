using Warehouse;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Key;
using System.Runtime.Serialization;
namespace Manager
{
    [DataContract]
    public class Creator: User
    {
        public Creator(string userName, long chartID) : base(userName, chartID)
        {
            SetRights(Rights.CreatorBot);
        }

        public async Task HandleCreatorBot(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.StartsWith(ConstKeyword.BEGINNING_NAME_STORE))
            {
                GroceryStore groceryStore = new GroceryStore(message.Chat.Username);
                string nameStore = message.Text[6..];
                groceryStore.Name = nameStore;
               
                FileXML.Store.Add(groceryStore);
                FileXML.SerializeStore();
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the description of the store with this context");
                await botClient.SendTextMessageAsync(message.Chat.Id, "description: ...");
                return;
            }
            else if (message.Text.StartsWith(ConstKeyword.BEGINNING_DESCRIPTION_STORE))
            {
                string description = message.Text[13..];
                Store groceryStore = FileXML.GetCreatedStore(message.Chat.Username);
                if (groceryStore == null)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "We don't have your bot. Pls create the bot.");
                    return;
                }
                groceryStore.Description = description;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Succeed!");
                await botClient.SendTextMessageAsync(message.Chat.Id,
                @$"You have a new comand for your store:
                {ConstKeyword.PERSON_STORE} {ConstKeyword.GET_CATALOG} {ConstKeyword.SET_CATALOG}");
                User user = FileXML.GetUserWithNull(message.Chat.Username, message.Chat.Id);
                Admin admin = new Admin(user.GetUserName(), user.GetChartID());
                FileXML.Store.Add(groceryStore);
                FileXML.SerializeStore();
                FileXML.SetUser(admin);
                return;
            }
            else
                await botClient.SendTextMessageAsync(message.Chat.Id, "Pls continue created bot!");
        }
    }
}
