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
using Warehouse;
using Warehouse.Manager;
using User = Warehouse.Manager.User;
using Key;
using Chepa.Bot.Db;
using System.Linq;
namespace Chepa.Bot
{
    class HandleTelegram
    {
        ChepaBotContext context = new ChepaBotContext();
        public async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                User user = context.Users.FirstOrDefault(u => u.UserName == update.Message.Chat.Username);//make with derived class unboxing
                if (user == null)
                {
                    if (update.Message.Chat.Username == null)
                    {
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"You don't have USERNAME. Pls make it.");
                        return;
                    }
                    user = new User(update.Message.Chat.Username, update.Message.Chat.Id);
                    context.Users.Add(user);
                    context.SaveChanges();
                }
                Rights myRights = user.Rights;
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
                            Creator creator = new Creator(user.UserName, user.ChatId, user.UserId);
                            StoresRepository storesRepository = new StoresRepository(context);
                            UserRepository userRepository = new UserRepository(context);
                            creator.StoreAddedEvent = storesRepository.Add;
                            creator.StoreUpdatedEvent = storesRepository.Update;
                            creator.CreatedStoreGetedEvent = storesRepository.GetCreatedStore;
                            creator.UsereUpdatedEvent = userRepository.Update;
                            await creator.HandleCreatorBot(botClient, update.Message);
                            return;
                        }
                    case Rights.Admin:
                        {
                            Admin admin = new Admin(user);
                            StoresRepository storesRepository = new StoresRepository(context);
                            UserRepository userRepository = new UserRepository(context);
                            admin.CatalogHandleEvent = HandleCatalog;
                            admin.MessageHandleEvent = HandleMessage;
                            admin.StoreAddedEvent = storesRepository.Add;
                            admin.OwnerStoresGetedEvent = storesRepository.GetOwnersStores;
                            admin.StoreUpdatedEvent = storesRepository.Update;
                            admin.UsereUpdatedEvent = userRepository.Update;
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
            else if (update.Type == UpdateType.CallbackQuery)
            {
                //include null
                User user = context.Users.FirstOrDefault(u => u.UserName == update.CallbackQuery.From.Username);//FileXML.GetUserWithNull(update.CallbackQuery.Message.Chat.Username, update.CallbackQuery.Message.Chat.Id);
                await HandleCallbackQuery(botClient, update.CallbackQuery, user);
            }
            return;
        }
        public async Task HandleBuyer(ITelegramBotClient botClient, Message message, User user)
        {
            StoresRepository storesRepository = new StoresRepository(context);
            UserRepository userRepository = new UserRepository(context);
            if (storesRepository.IsStore(message.Text[1..]))//without '/' 
            {
                await SetStoreId(botClient, message, user);
                userRepository.Update(user);
                if (user.StoreId != null)//cheak
                    await HandleGoods(botClient, user.ChatId, (GroceryStore)storesRepository.GetStore(user.StoreId));//make
            }
            else if (message.Text == ConstKeyword.ORDER)
            {
                Buyer buyer = new Buyer(user);
                buyer.Store = storesRepository.GetStoreForBuyer((int)buyer.StoreId);
                List<string> textItems = buyer.GetChoose();
                if (textItems.Count != 0)
                {
                    //make func
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Your check: {buyer.GetCheck()} and items:");
                    foreach (var item in textItems)
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"{item}");
                    buyer.RemoveBuyIteam(buyer.Store.Carts.FirstOrDefault(c => c.UserId == buyer.UserId).CartId);
                    storesRepository.Update(buyer.Store);


                }
                else
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"You don't choose goods");
            }
            else if (message.Text == ConstKeyword.START)
            {
                user = new User(message.Chat.Username, message.Chat.Id);
                userRepository.Update(user);
                await HandleMessage(botClient, message, user);
            }
            else
                await HandleMessage(botClient, message, user);
            return;
        }
        public async Task HandleStore(ITelegramBotClient botClient, Message message)
        {
            if (context.Store.Count() != 0)//check
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Choose store for visit");
                StringBuilder items = new StringBuilder();
                StoresRepository storesRepository = new StoresRepository(context);
                foreach (var item in storesRepository.GetAll())
                {
                    items.Append(ConstKeyword.SLACH);
                    items.Append(item.Name);
                    items.Append('\n');
                }
                await botClient.SendTextMessageAsync(message.Chat.Id, $"{items}");
            }
            else
                await botClient.SendTextMessageAsync(message.Chat.Id, "We don't have store");
            return;
        }
        public async Task SetStoreId(ITelegramBotClient botClient, Message message, User user)
        {
            StoresRepository storesRepository = new StoresRepository(context);
            foreach (var item in storesRepository.GetAll())
                if (message.Text == $"{ConstKeyword.SLACH}{item.Name}")
                {
                    user.StoreId = item.StoreId;
                    break;
                }
            if (user.StoreId != null)
                await botClient.SendTextMessageAsync(message.Chat.Id, "Make " + ConstKeyword.ORDER);
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Pls click on the store name");
                await botClient.SendTextMessageAsync(message.Chat.Id, "or we don't have this store.");
            }
            return;
        }

        public async Task HandleCatalog(ITelegramBotClient botClient, Message message, User user)
        {
            StoresRepository storesRepository = new StoresRepository(context);
            if (user != null && user.Rights == Rights.Buyer)
            {
                await SetStoreId(botClient, message, user);
                if (user.StoreId != null)
                    await HandleGoods(botClient, user.ChatId, (GroceryStore)user.Store);
            }
            else if (message.Text == ConstKeyword.SET_CATALOG)
            {
                InlineKeyboardButton[] keyboardButton = new InlineKeyboardButton[context.Store.Count()];
                var stores = storesRepository.GetOwnersStores(user.UserName);
                for (int i = 0; i < stores.Count; i++)
                    keyboardButton[i] = InlineKeyboardButton.WithCallbackData($"{stores[i].Name}", $"{ConstKeyword.CALLBACK_STORE_SET_CATALOG} {stores[i].Name}");
                InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardButton);
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Choose your store for adding product", replyMarkup: keyboard);
                Thread.Sleep(1);
            }
            else if (message.Text == ConstKeyword.GET_CATALOG)
            {
                InlineKeyboardButton[] keyboardButton = new InlineKeyboardButton[context.Store.Count()];//make func
                var stores = storesRepository.GetOwnersStores(user.UserName);
                for (int i = 0; i < stores.Count; i++)
                    keyboardButton[i] = InlineKeyboardButton.WithCallbackData($"{stores[i].Name}", $"{ConstKeyword.CALLBACK_STORE_GET_CATALOG} {stores[i].Name}");
                InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardButton);
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Choose your store for get products", replyMarkup: keyboard);
            }
            return;
        }
        public async Task HandleGoods(ITelegramBotClient botClient, long chatId, GroceryStore grocery)//make
        {
            if (grocery.Showcases != null)
            {
                foreach (var showcase in grocery.Showcases)
                {
                    InlineKeyboardButton[] keyboardButton = new InlineKeyboardButton[showcase.Products.Count];
                    string[] product = grocery.GetProducts(showcase.Name);
                    for (int j = 0; j < product.Length; j++)
                        keyboardButton[j] = InlineKeyboardButton.WithCallbackData($"{product[j]}", $"{ConstKeyword.CALLBACK_GOODS} {product[j]}");
                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(keyboardButton);
                    await botClient.SendTextMessageAsync(chatId, $"Catalog {showcase.Name}", replyMarkup: keyboard);
                }
                Thread.Sleep(1);
            }
            else
                await botClient.SendTextMessageAsync(chatId, $"The {grocery.Name} doesn't have catalog!");
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
                        StoresRepository storesRepository = new StoresRepository(context);
                        if (context.Store.Count() == 0)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "We don't have store");
                            return;
                        }
                        else
                            foreach (var store in storesRepository.GetAll())
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"{store}");
                        return;
                    }
                case ConstKeyword.PERSON_RIGHTS:
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"You are {user.Rights}");
                        return;
                    }
                case ConstKeyword.BUYER:
                    {
                        Buyer buyer = new Buyer(user);
                        UserRepository userRepository = new UserRepository(context);
                        userRepository.Update(buyer);
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"You are {buyer.Rights}");
                        await HandleStore(botClient, message);
                        return;
                    }
                case ConstKeyword.PERSON_STORE:
                    {
                        StoresRepository storesRepository = new StoresRepository(context);
                        List<Store> catalogStore = storesRepository.GetOwnersStores(user.UserName);
                        if (catalogStore != null)
                        {
                            Admin admin = new Admin(user);
                            UserRepository userRepository = new UserRepository(context);
                            userRepository.Update(admin);
                            admin.CatalogHandleEvent = HandleCatalog;
                            admin.MessageHandleEvent = HandleMessage;
                            admin.StoreAddedEvent = storesRepository.Add;
                            admin.OwnerStoresGetedEvent = storesRepository.GetOwnersStores;
                            admin.StoreUpdatedEvent = storesRepository.Update;
                            admin.UsereUpdatedEvent = userRepository.Update;
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
        public async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, User user)
        {
            if (user == null)
                return;
            if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_STORE_CREATE))
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "The store is being created");
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter the name of the store with this context");
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "name: store");
                Creator creator = new Creator(user.UserName, user.ChatId, user.UserId)
                {
                    Rights = Rights.CreatorBot
                };
                UserRepository userRepository = new UserRepository(context);
                userRepository.Update(creator);
                return;
            }
            else if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_GOODS))
                await HandleChooseBuyer(botClient, callbackQuery, user);
            else if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_STORE_SET_CATALOG))
            {
                string[] storeData = callbackQuery.Data.ToString().Split(' ');
                StoresRepository storesRepository = new StoresRepository(context);
                UserRepository userRepository = new UserRepository(context);
                List<Store> catalogStore = storesRepository.GetOwnersStores(user.UserName);
                string nameStore = null;
                if (user.Rights == Rights.Admin)
                {
                    for (int i = 0; i < catalogStore.Count; i++)
                        if (storeData[1] == catalogStore[i].Name)
                        {
                            nameStore = catalogStore[i].Name;
                            user.IsSetBuyItem = true;
                            user.StoreId = catalogStore[i].StoreId;
                            userRepository.Update(user);
                        }
                    if (user.IsSetBuyItem == true)
                    {
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"You choose: {nameStore}");
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter the description of the goods with this context");
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Goods: (name)(space)(price)(space)(showcases name)");
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"For end set goods print {ConstKeyword.END_INSTALLATION}");
                    }
                    else
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"We don't have this store");
                }
                else
                    await botClient.SendTextMessageAsync(user.ChatId, $"You don't have powers for this command: {callbackQuery.Data}");
                return;
            }
            else if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_STORE_GET_CATALOG))
            {
                if (user.Rights == Rights.Admin)
                {
                    var storesRepository = new StoresRepository(context);
                    string nameStore = callbackQuery.Data.Split(" ")[1];
                    var store = storesRepository.GetOwnersStores(user.UserName).FirstOrDefault(s => s.Name == nameStore);
                    await HandleGoods(botClient, user.ChatId, (GroceryStore)store);
                }
                else
                    await botClient.SendTextMessageAsync(user.ChatId,$"You don't have powers for this command: {callbackQuery.Data}");
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
        public async Task HandleChooseBuyer(ITelegramBotClient botClient, CallbackQuery callbackQuery, User user)
        {
            if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_GOODS))
            {
                string[] goodsWithNamePrice = callbackQuery.Data.ToString().Split(' ');
                if (user.Rights == Rights.Buyer)
                {
                    Buyer buyer = new Buyer(user);
                    StoresRepository storesRepository = new StoresRepository(context);
                    buyer.Store = storesRepository.GetStoreForBuyer((int)buyer.StoreId);
                    if (buyer.Store != null)
                    {
                        Cart cart = buyer.Store.Carts.FirstOrDefault(c => c.UserId == buyer.UserId && c.IsNewBuyIteamsFromBuyer == true);
                        if (cart == null)
                        {
                            cart = new Cart
                                    ((int)buyer.StoreId, buyer.UserId, Convert.ToBoolean(1), new Product(goodsWithNamePrice[1], Convert.ToInt32(goodsWithNamePrice[2])));
                        }
                        else
                            cart.AddItem(new Product(goodsWithNamePrice[1], Convert.ToInt32(goodsWithNamePrice[2])));
                        storesRepository.UpdateCart(cart);
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"You choose: {goodsWithNamePrice[1]}");
                    }
                    else
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "We don't have this store.");
                }
                else if (user.Rights == Rights.Admin)
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Admin choose: {goodsWithNamePrice[1]}");
                else
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"You don't have powers for this command.");

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
