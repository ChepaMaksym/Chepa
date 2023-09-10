using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Warehouse.Manager;
namespace Warehouse
{
    [KnownType(typeof(GroceryStore))]
    [DataContract]
    public abstract class Store
    {
        [DataMember()]
        protected string name;
        [DataMember()]
        protected string description;
        [DataMember()]
        protected List<Showcase> goodsCatalog;//make set or change or add
        protected List<Cart> basket = new List<Cart>();
        protected List<User> users = new List<User>();
        [DataMember()]
        private string userName;

        [Key]
        public int StoreId { get; set; }
        public string Owner { get => userName; set => userName = value; }
        public string Name { get => name; set => name = value; }
        public string Description { get => description; set => description = value; }

        public List<Showcase> Showcases { get => goodsCatalog; set => goodsCatalog = value; }
        public List<Cart> Carts { get => basket; set => basket = value; }
        public List<User> Users { get => users; set => users = value; }
        public Store()
        {

        }
        public Store(string userName)
        {
            this.userName = userName;
            foreach (var item in Carts)
            {
                item.Store = this;
            }
        }

        public abstract void SetCatalog(List<Product> goods, string nameShowcase);
        public abstract void BuyIteam(Cart cartWithStoreUserId);
        public abstract int GetCheck();
        public abstract List<Product> GetCatalog(string nameShowcase);
        public abstract List<string>? GetChooseBuyer();
        public abstract void RemoveCart(int cardId);

    }
}
