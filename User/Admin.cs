using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Warehouse;
using Key;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Warehouse.Manager
{
    [DataContract, KnownType(typeof(User))]
    public class Admin : User
    {
        public delegate Task MessageLoadedCallback(ITelegramBotClient botClient, Message message, User user);
        public delegate void StoreAddedHandler(Store store);
        public delegate List<Store> OwnerStoresGetedHandler(string owner);
        public delegate void StoreUpdatedHandler(Store store);
        public delegate void UserUpdatedHandler(User store);
        [NotMapped]
        public StoreAddedHandler StoreAddedEvent { get; set; }
        [NotMapped]
        public StoreUpdatedHandler StoreUpdatedEvent { get; set; }
        [NotMapped]
        public UserUpdatedHandler UsereUpdatedEvent { get; set; }
        [NotMapped]
        public MessageLoadedCallback CatalogHandleEvent { get; set; }
        [NotMapped]
        public MessageLoadedCallback MessageHandleEvent { get; set; }
        [NotMapped]
        public OwnerStoresGetedHandler OwnerStoresGetedEvent { get; set; }
        public Admin():base()
        {

        }
        public Admin(string userName, long chartID, int userId) : base(userName, chartID)
        {
            Rights = Rights.Admin;
            UserId = userId;
        }
        public Admin(User user):base(user)
        {
           Rights = Rights.Admin;
        }

        public async Task HandleAdmin(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == ConstKeyword.SET_CATALOG)
            {
                await CatalogHandleEvent(botClient, message, this);
                return;
            }
            else if (message.Text == ConstKeyword.END_INSTALLATION)
            {
                IsSetBuyItem = false;
                StoreId = null;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Ended add product");
                UsereUpdatedEvent(this);
            }
            else if (IsSetBuyItem)
                await HandleShowcase(botClient, message, this);
            else if (message.Text == ConstKeyword.GET_CATALOG)
            {
                if (OwnerStoresGetedEvent(UserName) == null)
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You don't have bot");
                else
                    await CatalogHandleEvent(botClient, message, this);
                return;
            }
            else if (message.Text == ConstKeyword.PERSON_STORE)
            {
                List<Store> catalogGroceryStore = OwnerStoresGetedEvent(UserName);
                if (catalogGroceryStore != null)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Your bot:");
                    foreach (var a in catalogGroceryStore)
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"{a}");
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Choose action: {ConstKeyword.SET_CATALOG} {ConstKeyword.GET_CATALOG}");
                }
                else
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You don't have bot");
                return;

            }
            else
                await MessageHandleEvent(botClient, message, this);
        }
        public async Task HandleShowcase(ITelegramBotClient botClient, Message message, User user)
        {
            if (message.Text.StartsWith(ConstKeyword.BEGINNING_GOODS))
            {
                string[] productWithShowcase = message.Text[7..].Split(' ');//without Goods:
                if (productWithShowcase.Length == 3)
                {
                    GroceryStore groceryStore = (GroceryStore)OwnerStoresGetedEvent(UserName).FirstOrDefault(s => user.StoreId == s.StoreId);
                    if (groceryStore == null)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "You don't have store.");
                        return;
                    }
                    groceryStore.AddProduct(new Product(productWithShowcase[0], Convert.ToInt32(productWithShowcase[1])), productWithShowcase[2]);
                    StoreUpdatedEvent(groceryStore);
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Repeat pls with correct context");
                    return;
                }
                await botClient.SendTextMessageAsync(message.Chat.Id, "Succeed!");
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id,
                              $"You choose with data: {message.Text}");
            }
            return;

        }
    }
}
