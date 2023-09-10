using System;
using System.Collections.Generic;
using System.Text;
using Warehouse;
using Warehouse.Manager;
using System.Linq;
namespace Chepa.Bot.Db
{
    public class CartRepository
    {
        private readonly ChepaBotContext context = new ChepaBotContext();
        public CartRepository(ChepaBotContext context)
        {
            this.context = context;
        }
        public void Add(Cart cart)
        {
            context.Carts.Add(cart);
            context.SaveChanges();
        }
        public Cart GetCartForSetBuyItem(int storeId, int userId)
        {
            Cart cart = context.Carts.FirstOrDefault(cart => cart.StoreId == storeId && cart.UserId == userId && cart.IsNewBuyIteamsFromBuyer == true);
            if (cart != null)
            {
                BuyItemRepository buyItemRepository = new BuyItemRepository(context);
                cart.BuyItem = buyItemRepository.GetCartBuyItems(cart.CartId);
                return cart;
            }
            return null;
        }
        public void Update(Cart cart)
        {
            var existingCart = context.Carts.Find(cart.CartId);
            if (existingCart != null)
            {
                existingCart.Check = cart.Check;
                existingCart.UserId = cart.UserId;
                existingCart.StoreId = cart.StoreId;
                existingCart.BuyItem = cart.BuyItem;
                existingCart.IsNewBuyIteamsFromBuyer = cart.IsNewBuyIteamsFromBuyer;
                context.SaveChanges();
            }
        }
        public void AddBuyItem(int cartId, BuyItem buyItem)
        {
            var existingCart = context.Carts.Find(cartId);
            if (existingCart != null)
            {
                if (existingCart.BuyItem != null)
                    existingCart.BuyItem.Add(buyItem);
                else
                    existingCart.BuyItem = new List<BuyItem>() { buyItem };
                context.SaveChanges();
            }
        }
        public void RemoveBuyItem(int cartId)
        {
            var existingCart = context.Carts.Find(cartId);
            if (existingCart != null)
            {
                if (existingCart.BuyItem != null)
                {
                    BuyItemRepository buyItemRepository = new BuyItemRepository(context);
                    existingCart.BuyItem.RemoveAll(buyItems => buyItemRepository.GetCartBuyItems(cartId).Contains(buyItems));
                    context.SaveChanges();
                }
            }
        }
        public void Delete(int cartId)
        {
            var cartToDelete = context.Carts.Find(cartId);
            if (cartToDelete != null)
            {
                context.Carts.Remove(cartToDelete);
                context.SaveChanges();
            }
        }
    }
}
