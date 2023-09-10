using System.Collections.Generic;
using System.Linq;
using Warehouse;
namespace Chepa.Bot.Db
{
    public class StoresRepository
    {
        private readonly ChepaBotContext context = new ChepaBotContext();

        public StoresRepository(ChepaBotContext context)
        {
            this.context = context;
        }
        public IQueryable<Store> GetAll()
        {
            return context.Store;
        }
        public List<Store> GetOwnersStores(string userName)//twice repeat
        {
            var ownersStores = context.Store.Where(store => store.Owner == userName).ToList();
            for (int i = 0; i < ownersStores.Count; i++)
                ownersStores[i] = GetStoreWithShowcasesProductsCartsBuyItems(ownersStores[i]);
            return ownersStores;
        }
        public string GetCatalog()
        {
            var stores = context.Store;
            string catalogStore = null;
            foreach (var store in stores)
                catalogStore += $"{store.Name}\n";
            return catalogStore;
        }
        public bool IsStore(string clearStoreName)
        {
            var stores = context.Store.FirstOrDefault(s => s.Name == clearStoreName);
            if (stores != null)
                return true;
            return false;
        }
        public Store GetStore(int? id)
        {
            return GetStoreWithShowcasesProductsCartsBuyItems(context.Store.Find(id));
        }
        public Store GetStoreWithShowcasesProductsCartsBuyItems(Store store)
        {
            if (store != null)
            {
                ShowcasesRepository showcasesRepository = new ShowcasesRepository(context);
                ProductsRepository productsRepository = new ProductsRepository(context);
                var showcases = context.Showcases.Where(showcases => showcases.StoreId == store.StoreId).ToList();
                for (int j = 0; j < showcases.Count; j++)
                {
                    var products = context.Products.Where(products => products.ShowcasesId == showcases[j].ShowcaseId).ToList();
                    showcases[j].Products = products;
                }
                CartRepository cartRepository = new CartRepository(context);
                BuyItemRepository buyItemRepository = new BuyItemRepository(context);
                var carts = context.Carts.Where(cart => cart.StoreId == store.StoreId).ToList();
                for (int j = 0; j < carts.Count; j++)
                {
                    var buyItem = context.BuyItems.Where(buyItem => buyItem.CartId == carts[j].CartId).ToList();
                    carts[j].BuyItem = buyItem;
                }
                store.Showcases = showcases;
                store.Carts = carts;
                return store;
            }
            return null;
        }
        public Store GetStoreForBuyer(int id)
        {
            return GetStoreWithCartsBuyItems(context.Store.Find(id));
        }
        public Store GetStoreWithCartsBuyItems(Store store)
        {
            if (store != null)
            {
                CartRepository cartRepository = new CartRepository(context);
                BuyItemRepository buyItemRepository = new BuyItemRepository(context);
                var carts = context.Carts.Where(cart => cart.StoreId == store.StoreId && cart.IsNewBuyIteamsFromBuyer == true).ToList();
                for (int j = 0; j < carts.Count; j++)
                {
                    var buyItem = context.BuyItems.Where(buyItem => buyItem.CartId == carts[j].CartId).ToList();
                    carts[j].BuyItem = buyItem;
                }
                store.Carts = carts;
                return store;
            }
            return null;
        }
        public void SetCart(Cart cart)
        {
            var existingStore = context.Store.Find(cart.StoreId);
            if (existingStore != null)
            {
                if (existingStore.Carts != null)
                    existingStore.Carts.Add(cart);
                else
                    existingStore.Carts = new List<Cart>() { cart };
                context.SaveChanges();
            }
            context.SaveChanges();
        }

        public void UpdateOwner(int storeId,string owner)
        {
            var existingStore = context.Store.Find(storeId);
            if (existingStore != null)
            {
                existingStore.Owner = owner;
                context.SaveChanges();
            }
        }
        public void Add(Store store)
        {
            context.Store.Add(store);
            context.SaveChanges();
        }
        public Store GetCreatedStore(string Owner)
        {
            var stores = context.Store.FirstOrDefault(s => s.Owner == Owner && s.Description == null);
            if (stores != null)
                return stores;
            return null;
        }
        public void Update(Store store)
        {
            var existingStore = context.Store.Find(store.StoreId);

            if (existingStore != null)
            {
                existingStore.Name = store.Name;
                existingStore.Description = store.Description;
                existingStore.Showcases = store.Showcases;
                existingStore.Carts = store.Carts;
                context.SaveChanges();
            }
        }
        public void UpdateCart(Cart cart)
        {
            var existingStore = context.Store.Find(cart.StoreId);

            if (existingStore != null)
            {
                CartRepository cartRepository = new CartRepository(context);
                cartRepository.Update(cart);
                existingStore.Carts.Add(cart);
                context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var storeToDelete = context.Store.Find(id);
            if (storeToDelete != null)
            {
                context.Store.Remove(storeToDelete);
                context.SaveChanges();
            }
        }
    }
}
