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
using Microsoft.EntityFrameworkCore;

namespace Chepa.Bot
{
    public class HandleTelegram
    {
        ChepaBotContext context;
        public HandleTelegram()
        {
            context = new ChepaBotContext();
        }
        public HandleTelegram(ChepaBotContext chepaBotContext)
        {
            context = chepaBotContext;
        }
        public async Task HandleUpdates(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
                await HandleMessageUpdate(botClient, update);

            else if (update.Type == UpdateType.CallbackQuery)
                await HandleCallbackQuery(botClient, update);
        }
        private async Task HandleMessageUpdate(ITelegramBotClient botClient, Update update)
        {
            User user = await CreateUserIfNotExist(botClient, update);

            Console.WriteLine(
                $"Start {update.Message.Chat.Username} | {update.Message.Chat.FirstName} | {update.Message.Chat.LastName} | {update.Message.Date}.");

            await HandleUserRights(botClient, update, user);

            Console.WriteLine(
                $"End   {update.Message.Chat.Username} | {update.Message.Date}.");
        }
        public async Task<User> CreateUserIfNotExist(ITelegramBotClient botClient, Update update)
        {
            User user = await context.Users.FirstOrDefaultAsync(u => u.UserName == update.Message.Chat.Username);
            if (user == null)
            {

                if (update.Message.Chat.Username == null)
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"You don't have USERNAME. Pls make it.");
                    throw new InvalidOperationException("User doesn't have a username.");
                }

                user = new User(update.Message.Chat.Username, update.Message.Chat.Id);
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
            return user;
        }

        private async Task HandleCallbackQuery(ITelegramBotClient botClient, Update update)
        {
            User user = await context.Users.FirstOrDefaultAsync(u => u.UserName == update.CallbackQuery.Message.Chat.Username);
            if (user != null)
                await HandleCallbackQueryUserChoose(botClient, update.CallbackQuery, user);
            else
            {
                await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "We can't find you. Repeat with /start");
                throw new InvalidOperationException($"User: ({update.Message.Chat.Username}) doesn't exist in DB.");
            }
        }

        public async Task HandleUserRights(ITelegramBotClient botClient, Update update, User user)
        {
            switch (user.Rights)
            {
                case Rights.Watcher:
                    await HandleMessage(botClient, update.Message, user);
                    break;
                case Rights.Buyer:
                    await HandleBuyer(botClient, update.Message, user);
                    break;
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
                        break;
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
                        break;
                    }
                default:
                    if (user.Rights == Rights.AllRights)
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "You have all rights");
                    break;
            }
        }
        public async Task HandleBuyer(ITelegramBotClient botClient, Message message, User user)
        {
            StoresRepository storesRepository = new StoresRepository(context);
            UserRepository userRepository = new UserRepository(context);

            if (storesRepository.IsStore(message.Text[1..])) // without '/'
                await HandleStoreForBuyer(botClient, message, user, storesRepository, userRepository);

            else if (message.Text == ConstKeyword.ORDER)
                await HandleBuyerOrder(botClient, message, user, storesRepository);

            else if (message.Text == ConstKeyword.START)
                await HandleBuyerStart(botClient, message, userRepository);

            else
                await HandleMessage(botClient, message, user);
        }

        private async Task HandleStoreForBuyer(ITelegramBotClient botClient, Message message, User user, StoresRepository storesRepository, UserRepository userRepository)
        {
            user = await SetStoreId(botClient, message, user);
            userRepository.Update(user);

            if (user.StoreId != null)
            {
                await HandleBuyerProducts(botClient, user, storesRepository);
            }
        }

        private async Task HandleBuyerOrder(ITelegramBotClient botClient, Message message, User user, StoresRepository storesRepository)
        {
            Buyer buyer = new Buyer(user);
            buyer.Store = storesRepository.GetStoreForBuyer((int)buyer.StoreId);
            List<string> textItems = buyer.GetChoose();

            if (textItems.Count != 0)
            {
                await SendBuyerCheck(botClient, message, buyer, textItems);
                buyer.RemoveBuyIteam(buyer.Store.Carts.FirstOrDefault(c => c.UserId == buyer.UserId).CartId);
                storesRepository.Update(buyer.Store);
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "You don't choose goods");
            }
        }

        private async Task HandleBuyerStart(ITelegramBotClient botClient, Message message, UserRepository userRepository)
        {
            User newUser = new User(message.Chat.Username, message.Chat.Id);
            userRepository.Update(newUser);
            await HandleMessage(botClient, message, newUser);
        }

        private async Task HandleBuyerProducts(ITelegramBotClient botClient, User user, StoresRepository storesRepository)
        {
            await HandleProduct(botClient, user.ChatId, (GroceryStore)storesRepository.GetStore(user.StoreId));
        }

        private async Task SendBuyerCheck(ITelegramBotClient botClient, Message message, Buyer buyer, List<string> textItems)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Your check: {buyer.GetCheck()} and items:");
            foreach (var item in textItems)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"{item}");
            }
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
        public async Task<User> SetStoreId(ITelegramBotClient botClient, Message message, User user)
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
            return user;

        }

        public async Task HandleCatalog(ITelegramBotClient botClient, Message message, User user)
        {
            StoresRepository storesRepository = new StoresRepository(context);
            if (user != null && user.Rights == Rights.Buyer)
            {
                user = await SetStoreId(botClient, message, user);
                if (user.StoreId != null)
                    await HandleProduct(botClient, user.ChatId, (GroceryStore)user.Store);
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
        public async Task HandleProduct(ITelegramBotClient botClient, long chatId, GroceryStore grocery)//make
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
                    await HandleStart(botClient, message);
                    break;
                case ConstKeyword.INLINE:
                    await HandleInline(botClient, message);
                    break;
                case ConstKeyword.CATALOG_STORE:
                    await HandleCatalogStore(botClient, message);
                    break;
                case ConstKeyword.PERSON_RIGHTS:
                    await HandlePersonRights(botClient, user, message);
                    break;
                case ConstKeyword.BUYER:
                    await HandleBuyer(botClient, user, message);
                    break;
                case ConstKeyword.PERSON_STORE:
                    await HandlePersonStore(botClient, user, message);
                    break;
                default:
                    await HandleDefault(botClient, message);
                    break;
            }
        }

        public async Task HandleStart(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Choose commands:" +
                $" {ConstKeyword.INLINE} | {ConstKeyword.CATALOG_STORE} | {ConstKeyword.PERSON_RIGHTS} | {ConstKeyword.BUYER} | {ConstKeyword.PERSON_STORE}");
        }

        public async Task HandleInline(ITelegramBotClient botClient, Message message)
        {
            InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(ConstKeyword.CALLBACK_STORE_CREATE),
                },
            });
            await botClient.SendTextMessageAsync(message.Chat.Id, "Create store:", replyMarkup: keyboard);
        }

        public async Task HandleCatalogStore(ITelegramBotClient botClient, Message message)
        {
            StoresRepository storesRepository = new StoresRepository(context);
            if (context.Store.Count() == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "We don't have store");
                throw new InvalidOperationException("Stores don't exist.");
            }
            foreach (var store in storesRepository.GetAll())
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"{store}");
            }
        }

        public async Task HandlePersonRights(ITelegramBotClient botClient, User user, Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, $"You are {user.Rights}");
        }

        public async Task HandleBuyer(ITelegramBotClient botClient, User user, Message message)
        {
            Buyer buyer = new Buyer(user);
            UserRepository userRepository = new UserRepository(context);
            userRepository.Update(buyer);
            await botClient.SendTextMessageAsync(message.Chat.Id, $"You are {buyer.Rights}");
            await HandleStore(botClient, message);
        }

        public async Task HandlePersonStore(ITelegramBotClient botClient, User user, Message message)
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
        }

        public async Task HandleDefault(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, $"{message.From.Username} choose with data: {message.Text}");
        }
        public async Task HandleCallbackQueryUserChoose(ITelegramBotClient botClient, CallbackQuery callbackQuery, User user)
        {
            if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_STORE_CREATE))
                await HandleCallbackStoreCreate(botClient, callbackQuery, user);

            else if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_GOODS))
                await HandleChooseBuyer(botClient, callbackQuery, user);

            else if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_STORE_SET_CATALOG))
                await HandleCallbackStoreSetCatalog(botClient, callbackQuery, user);

            else if (callbackQuery.Data.StartsWith(ConstKeyword.CALLBACK_STORE_GET_CATALOG))
                await HandleCallbackStoreGetCatalog(botClient, callbackQuery, user);

            else
                await HandleDefaultCallback(botClient, callbackQuery);
        }

        public async Task HandleCallbackStoreCreate(ITelegramBotClient botClient, CallbackQuery callbackQuery, User user)
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
        }

        public async Task HandleCallbackStoreSetCatalog(ITelegramBotClient botClient, CallbackQuery callbackQuery, User user)
        {
            string[] storeData = callbackQuery.Data.ToString().Split(' ');
            StoresRepository storesRepository = new StoresRepository(context);
            UserRepository userRepository = new UserRepository(context);
            List<Store> catalogStore = storesRepository.GetOwnersStores(user.UserName);
            string nameStore = null;

            if (user.Rights == Rights.Admin)
            {
                for (int i = 0; i < catalogStore.Count; i++)
                {
                    if (storeData[1] == catalogStore[i].Name)
                    {
                        nameStore = catalogStore[i].Name;
                        user.IsSetBuyItem = true;
                        user.StoreId = catalogStore[i].StoreId;
                        userRepository.Update(user);
                    }
                }

                if (user.IsSetBuyItem == true)
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"You choose: {nameStore}");
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Enter the description of the goods with this context");
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Goods: (name)(space)(price)(space)(showcases name)");
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"For end set goods print {ConstKeyword.END_INSTALLATION}");
                }
                else
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"We don't have this store");
                    throw new InvalidOperationException($"The owner of this store ({user.UserName}) no longer has access to it.");
                }
            }
            else
                await botClient.SendTextMessageAsync(user.ChatId, $"You don't have powers for this command: {callbackQuery.Data}");
        }

        public async Task HandleCallbackStoreGetCatalog(ITelegramBotClient botClient, CallbackQuery callbackQuery, User user)
        {
            if (user.Rights == Rights.Admin)
            {
                var storesRepository = new StoresRepository(context);
                string nameStore = callbackQuery.Data.Split(" ")[1];
                var store = storesRepository.GetOwnersStores(user.UserName).FirstOrDefault(s => s.Name == nameStore);
                await HandleProduct(botClient, user.ChatId, (GroceryStore)store);
            }
            else
            {
                await botClient.SendTextMessageAsync(user.ChatId, $"You don't have powers for this command: {callbackQuery.Data}");
                throw new InvalidOperationException("User don't have permission"); 
            }
        }

        public async Task HandleDefaultCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"You choose with data: {callbackQuery.Data}");
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
                            cart = new Cart
                                    ((int)buyer.StoreId, buyer.UserId, Convert.ToBoolean(1), new Product(goodsWithNamePrice[1], Convert.ToInt32(goodsWithNamePrice[2])));
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
