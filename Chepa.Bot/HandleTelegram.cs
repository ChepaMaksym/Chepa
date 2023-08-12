using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Store;
using Manager;
using User = Manager.User;
using Key;
namespace Chepa.Bot
{
    class HandleTelegram
    {
        public async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                User user = FileXML.GetUserWithNull(update.Message.Chat.Username, update.Message.Chat.Id);//make with derived class unboxing
                if (user == null)
                {
                    if (update.Message.Chat.Username == null)
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"You don't have USERNAME");
                    user = new User(update.Message.Chat.Username, update.Message.Chat.Id);
                    FileXML.SetUser(user);
                }
                Rights myRights = user.GetRights();
                Console.WriteLine(
                $"{update.Message.Chat.Username}  |  {update.Message.Chat.FirstName}  |  {update.Message.Chat.LastName}  |  {update.Message.Date}.");
                switch (myRights)
                {
                    case Rights.Watcher:
                        await HandleMessage(botClient, update.Message, user);
                        return;
                    case Rights.Buyer:
                        await HandleBuyer(botClient, update.Message, user);
                        return;
                    case Rights.CreatorBot:
                        {
                            Creator creator = (Creator)user;
                            await creator.HandleCreatorBot(botClient, update.Message);
                            return;
                        }
                    case Rights.Admin:
                        {
                            Admin admin = (Admin)user;
                            admin.CatalogHandler = HandleCatalog;
                            admin.MessageHandler = HandleMessage;
                            await admin.HandleAdmin(botClient, update.Message);
                            return;
                        }
                    default:
                        {
                            if (myRights == Rights.AllRights)
                                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "You have all rights");
                            return;
                        }
                }
            }
            else if (update.Type == UpdateType.CallbackQuery && FileXML.GetUserWithNull(update.CallbackQuery.Message.Chat.Username, update.CallbackQuery.Message.Chat.Id) != null)
                await HandleCallbackQuery(botClient, update.CallbackQuery);
            return;
        }
        public async Task HandleBuyer(ITelegramBotClient botClient, Message message, User user)
        {
            if (FileXML.DeserializeStore() != null)
            {
                if (FileXML.IsStore(message.Text[1..]))//without '/' set index store
                {
                    Buyer buyer = new Buyer(user.GetUserName(), user.GetChartID());
                    await SetStoreForBuyer(botClient, message, buyer);
                    if (buyer.GetIndexStore() != -1)
                        await HandleGoods(botClient, message, FileXML.GetStore(buyer.GetIndexStore()));
                }
                else if (message.Text == ConstKeyword.ORDER)
                {
                    Buyer buyer = (Buyer)FileXML.GetUserWithNull(message.Chat.Username, message.Chat.Id);
                    string[] textItems = buyer.GetChoose();
                    if (textItems != null)
                    {
                        //make func
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Your check: {buyer.GetCheck()} and items:");
                        foreach (var item in textItems)
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"{item}");
                        buyer.RemoveBuyIteam();
                        FileXML.SetUser(buyer);
                    }
                    else
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"You don't choose goods");
                }
                else if (message.Text == ConstKeyword.START)
                {
                    user = new User(message.Chat.Username, message.Chat.Id);
                    FileXML.SetUser(user);
                    await HandleMessage(botClient, message, user);
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Sorry we don't have store");
                await HandleMessage(botClient, message,user);
            }
            return;
        }
        public async Task HandleStore(ITelegramBotClient botClient, Message message)
        {
            if (FileXML.DeserializeStore() != null)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Choose store for visit");
                List<GroceryStore> catalogGroceryStore = FileXML.DeserializeStore();
                StringBuilder items = new StringBuilder();
                for (int i = 0; i < catalogGroceryStore.Count; i++)
                {
                    items.Append(ConstKeyword.SLACH);
                    items.Append(catalogGroceryStore[i].GetName());
                    items.Append('\n');
                }
                await botClient.SendTextMessageAsync(message.Chat.Id, $"{items}");
            }
            else
                await botClient.SendTextMessageAsync(message.Chat.Id, "We don't have store");
            return;
        }
        public async Task SetStoreForBuyer(ITelegramBotClient botClient, Message message, Buyer buyer)
        {
            bool isComandStore = false;
            int indexStore = -1;
            List<GroceryStore> catalogStore = FileXML.DeserializeStore();
            for (int i = 0; i < catalogStore.Count; i++)
                if (message.Text == $"{ConstKeyword.SLACH}{catalogStore[i].GetName()}")
                {
                    isComandStore = true;
                    indexStore = i;
                    break;
                }

            if (isComandStore)
            {
                buyer.SetIndexStore(indexStore);
                buyer.SetStore(catalogStore[indexStore]);
                FileXML.SetUser(buyer);
                await botClient.SendTextMessageAsync(message.Chat.Id, "Make" + ConstKeyword.ORDER);
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Pls click on the store name");
                await botClient.SendTextMessageAsync(message.Chat.Id, "or we don't have this store.");
            }
            return;
        }
        //here
        public async Task HandleCatalog(ITelegramBotClient botClient, Message message, User user)
        {
            if (user != null && user.GetRights() == Rights.Buyer)
            {
                await SetStoreForBuyer(botClient, message, (Buyer)user);
                if (user.GetIndexStore() != -1)
                    await HandleGoods(botClient, message, FileXML.GetStore(user.GetIndexStore()));
            }
            else if (message.Text == ConstKeyword.SET_CATALOG)
            {
                List<GroceryStore> catalogStore = FileXML.GetStoreWithNull(message.Chat.Username);
                InlineKeyboardButton[] keyboardButton = new InlineKeyboardButton[catalogStore.Count];
                int count = FileXML.DeserializeStore().Count;
                for (int i = 0; i < count; i++)
                    keyboardButton[i] = InlineKeyboardButton.WithCallbackData($"{catalogStore[i].GetName()}", $"{ConstKeyword.CALLBACK_STORE_CATALOG} {catalogStore[i].GetName()}");
                InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardButton);
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Choose your store for adding goods", replyMarkup: keyboard);
                Thread.Sleep(1);
            }
            else
            {
                List<GroceryStore> catalogStore = FileXML.GetStoreWithNull(message.Chat.Username);
                for (int i = 0; i < catalogStore.Count; i++)
                    await HandleGoods(botClient, message, catalogStore[i]);
            }
            return;
        }
        public async Task HandleGoods(ITelegramBotClient botClient, Message message, GroceryStore grocery)
        {
            if (grocery.GetCatalogInfo() != null)
            {
                string[] catalog = grocery.GetCatalogInfo();
                InlineKeyboardButton[] keyboardButton = new InlineKeyboardButton[catalog.Length];
                for (int i = 0; i < catalog.Length; i++)
                    keyboardButton[i] = InlineKeyboardButton.WithCallbackData($"{catalog[i]}", $"{ConstKeyword.CALLBACK_GOODS} {catalog[i]}");
                InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardButton);
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Catalog {grocery.GetName()}", replyMarkup: keyboard);
                Thread.Sleep(1);
            }
            else
                await botClient.SendTextMessageAsync(message.Chat.Id, $"The {grocery.GetName()} doesn't have catalog!");
            return;
        }

        public async Task HandleBuyItem(ITelegramBotClient botClient, Message message, int indexStore)
        {
            if (message.Text.StartsWith(ConstKeyword.BEGINNING_GOODS))
            {
                string[] goods = message.Text[7..].Split(' ');
                if (goods.Length == 2)
                {
                    List<GroceryStore> groceryStore = FileXML.GetStoreWithNull(message.Chat.Username);
                    groceryStore[indexStore].SetGoods(new Goods(goods[0], Convert.ToInt32(goods[1])));
                    FileXML.AddCatalogStore(message.Chat.Username, groceryStore[indexStore]);
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

        public async Task HandleMessage(ITelegramBotClient botClient, Message message, User user)
        {
            switch (message.Text)
            {
                case ConstKeyword.START:
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Choose commands:" +
                            $" {ConstKeyword.INLINE} | {ConstKeyword.CATALOG_STORE} | {ConstKeyword.PERSON_RIGHTS} | {ConstKeyword.BUYER} | {ConstKeyword.PERSON_STORE}");
                        return;
                    }
                case ConstKeyword.INLINE:
                    {
                        InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(ConstKeyword.CALLBACK_STORE_CREATE),
                        },
                        });
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Create store:", replyMarkup: keyboard);
                        return;
                    }
                case ConstKeyword.CATALOG_STORE:
                    {
                        List<GroceryStore> catalogGroceryStore = FileXML.DeserializeStore();
                        if (catalogGroceryStore == null)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "We don't have store");
                            return;
                        }
                        else
                            foreach (var a in catalogGroceryStore)
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"{a}");
                        return;
                    }
                case ConstKeyword.PERSON_RIGHTS:
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"You are {user.GetRights()}");
                        return;
                    }
                case ConstKeyword.BUYER:
                    {
                        Buyer buyer = new Buyer(user.GetUserName(), user.GetChartID());
                        FileXML.SetUser(buyer);
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"You are {buyer.GetRights()}");
                        await HandleStore(botClient, message);
                        return;
                    }
                case ConstKeyword.PERSON_STORE:
                    {
                        if (FileXML.GetStoreWithNull(message.Chat.Username) != null)
                        {
                            Admin admin = new Admin(user.GetUserName(), user.GetChartID());
                            FileXML.SetUser(admin);
                            admin.CatalogHandler = HandleCatalog;
                            admin.MessageHandler = HandleMessage;
                            await admin.HandleAdmin(botClient, message);
                        }
                        else
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"You don't have store");
                        return;
                    }
                default:
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id,
                            $"{message.From.Username} choose with data: {message.Text}");
                        return;
                    }
            }
        }
        public async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_STORE_CREATE))
            {

                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "The store is being created");
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter the name of the store with this context");
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "name: store");
                User user = FileXML.GetUserWithNull(callbackQuery.Message.Chat.Username, callbackQuery.Message.Chat.Id);
                Creator creator = new Creator(user.GetUserName(), user.GetChartID());
                FileXML.SetUser(creator);
                return;
            }
            else if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_GOODS))
            {
                if (FileXML.GetUserWithNull(callbackQuery.Message.Chat.Username, callbackQuery.Message.Chat.Id) != null)
                    await HandleChooseBuyer(botClient, callbackQuery);
            }
            else if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_STORE_CATALOG))
            {
                string[] store = callbackQuery.Data.ToString().Split(' ');
                List<GroceryStore> catalogGroceryStore = FileXML.DeserializeStore();
                Admin admin = (Admin)FileXML.GetUserWithNull(callbackQuery.Message.Chat.Username, callbackQuery.Message.Chat.Id);
                for (int i = 0; i < catalogGroceryStore.Count; i++)
                    if (store[1] == catalogGroceryStore[i].GetName())
                    {
                        admin.isSetBuyItem = true;
                        admin.SetIndexStore(i);
                    }
                if (admin.isSetBuyItem == true)
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"You choose: {catalogGroceryStore[admin.GetIndexStore()].GetName()}");
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter the description of the goods with this context");
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Goods: (name)(space)(price)");
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"For end set goods print {ConstKeyword.END_INSTALLATION}");
                    FileXML.SetUser(admin);
                }
                else
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"We don't have this store");
                return;
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    $"You choose with data: {callbackQuery.Data}"
                    );
                return;
            }
        }
        public async Task HandleChooseBuyer(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_GOODS))
            {
                User user = FileXML.GetUserWithNull(callbackQuery.Message.Chat.Username, callbackQuery.Message.Chat.Id);
                string[] goods = callbackQuery.Data.ToString().Split(' ');
                if (user is Buyer)
                {
                    Buyer buyer = (Buyer)user;
                    buyer.ChooseIteams(new Goods(goods[1], Convert.ToInt32(goods[2])));
                    FileXML.SetUser(buyer);
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"You choose: {goods[1]}");
                }
                else if (user is Admin)
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Admin choose: {goods[1]}");
                }
            }

        }
        public Task HandleError(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)// make more feature: right's buyer,admin;uconnect;wait;chart ID; 
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Помилка:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
