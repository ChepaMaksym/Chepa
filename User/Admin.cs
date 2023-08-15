using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Warehouse;
using Key;
using System.Runtime.Serialization;

namespace Manager
{
    [DataContract, KnownType(typeof(User))]
    public class Admin : User
    {
        public Admin(string userName, long chartID) : base(userName, chartID)
        {
            SetRights(Rights.Admin);
        }

        public delegate Task MessageLoadedCallback(ITelegramBotClient botClient, Message message, User user);
        public MessageLoadedCallback CatalogHandler { get; set; }
        public MessageLoadedCallback MessageHandler { get; set; }
        public async Task HandleAdmin(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == ConstKeyword.SET_CATALOG)
            {
                IsSetBuyItem = true;
                await CatalogHandler(botClient, message, this);
                return;
            }
            else if (message.Text == ConstKeyword.END_INSTALLATION)
            {
                IsSetBuyItem = false;
                SetIndexStore(-1);
                FileXML.SetUser(this);
            }
            else if (IsSetBuyItem)
                await HandleBuyItem(botClient, message, FileXML.GetUserWithNull(message.Chat.Username, message.Chat.Id).GetIndexStore());
            else if (message.Text == ConstKeyword.GET_CATALOG)
            {
                if (FileXML.GetStoreWithNull(message.Chat.Username) == null)
                    await botClient.SendTextMessageAsync(message.Chat.Id, "You don't have bot");
                else
                    await CatalogHandler(botClient, message, this);
                return;
            }
            else if (message.Text == ConstKeyword.PERSON_STORE)
            {
                List<Store> catalogGroceryStore = FileXML.GetStoreWithNull(message.Chat.Username);
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
                await MessageHandler(botClient, message, this);
        }
        public async Task HandleBuyItem(ITelegramBotClient botClient, Message message, int indexStore)
        {
            if (message.Text.StartsWith(ConstKeyword.BEGINNING_GOODS))
            {
                string[] goods = message.Text[7..].Split(' ');
                if (goods.Length == 2)
                {
                    GroceryStore groceryStore = (GroceryStore)FileXML.GetStoreWithNull(message.Chat.Username,indexStore);
                    groceryStore.SetGoods(new Goods(goods[0], Convert.ToInt32(goods[1])));
                    FileXML.AddCatalogStore(message.Chat.Username, groceryStore);
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
