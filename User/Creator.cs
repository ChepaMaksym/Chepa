using Warehouse;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Key;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Warehouse.Manager
{
    [DataContract]
    public class Creator: User
    {
        public delegate void StoreAddedHandler(Store store);
        public delegate void StoreUpdatedHandler(Store store);
        public delegate void UserUpdatedHandler(User store);
        public delegate Store CreatedStoreGetHandler(string owner);
        [NotMapped]
        public StoreAddedHandler StoreAddedEvent { get; set; }
        [NotMapped]
        public CreatedStoreGetHandler CreatedStoreGetedEvent { get; set; }
        [NotMapped]
        public StoreUpdatedHandler StoreUpdatedEvent { get; set; }  
        [NotMapped]
        public UserUpdatedHandler UsereUpdatedEvent { get; set; }
        public Creator() : base()
        {

        }
        public Creator(string userName, long chartID, int userId) : base(userName, chartID)
        {
            Rights = Rights.CreatorBot;
            UserId = userId;
        }

        public async Task HandleCreatorBot(ITelegramBotClient botClient, Message message)
        {
            if (message.Text.StartsWith(ConstKeyword.BEGINNING_NAME_STORE))
            {
                GroceryStore groceryStore = new GroceryStore(message.Chat.Username);
                string nameStore = message.Text[6..];
                groceryStore.Name = nameStore;
                StoreAddedEvent(groceryStore);
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the description of the store with this context");
                await botClient.SendTextMessageAsync(message.Chat.Id, "description: ...");
                return;
            }
            else if (message.Text.StartsWith(ConstKeyword.BEGINNING_DESCRIPTION_STORE))
            {
                string description = message.Text[13..];
                Store groceryStore = CreatedStoreGetedEvent(UserName);
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
                Admin admin = new Admin(UserName, ChatId, UserId);
                StoreUpdatedEvent(groceryStore);
                UsereUpdatedEvent(admin);
                return;
            }
            else
                await botClient.SendTextMessageAsync(message.Chat.Id, "Pls continue created bot!");
        }
    }
}
