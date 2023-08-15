using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Warehouse
{
    [KnownType(typeof(GroceryStore))]
    [DataContract]
    public abstract class Store
    {
        [DataMember()]
        private Cart cart;//make set or change or add
        [DataMember()]
        private string name;
        [DataMember()]
        private string description;
        [DataMember()]
        private Group group;//make set or change or add
        [DataMember()]
        private readonly string userName;

        public string UserName => userName;
        public string Name { get => name; set => name = value; }
        public Cart Cart { get => cart; set => cart = value; }
        public Group Group { get => group; set => group = value; }
        public string Description { get => description; set => description = value; }

        public Store(string userName)
        {
            this.userName = userName;
        }

        public abstract void SetCatalog(List<Goods> goods);
        public abstract void BuyIteam(Goods goods, int amount);
        public abstract int GetCheck();
        public abstract List<Goods> GetCatalog();

    }
}
