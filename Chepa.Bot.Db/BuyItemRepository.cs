using System.Collections.Generic;
using System.Linq;
using Warehouse;
namespace Chepa.Bot.Db
{
    public class BuyItemRepository
    {
        private readonly ChepaBotContext context = new ChepaBotContext();

        public BuyItemRepository(ChepaBotContext chepaBotContext)
        {
            context = chepaBotContext;
        }
        public IQueryable<BuyItem> GetAll()
        {
            return context.BuyItems;
        }

        public BuyItem GetBuyItem(int id)
        {
            return context.BuyItems.Find(id);
        }
        public List<BuyItem> GetCartBuyItems(int cartId)
        {
            return context.BuyItems.Where(b => b.CartId == cartId).ToList();
        }

        public void Add(BuyItem buyItem)
        {
            context.BuyItems.Add(buyItem);
            context.SaveChanges();
        }
        public void Update(BuyItem product)
        {
            var existingBuyItem = context.BuyItems.Find(product.BuyItemId);
            if (existingBuyItem != null)
            {
                existingBuyItem.Name = product.Name;
                existingBuyItem.Price = product.Price;
                existingBuyItem.Amount = product.Amount;
                context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var buyItemToDelete = context.BuyItems.Find(id);

            if (buyItemToDelete != null)
            {
                context.BuyItems.Remove(buyItemToDelete);
                context.SaveChanges();
            }
        }
    }
}
