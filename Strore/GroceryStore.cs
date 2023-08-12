using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Store
{
    [DataContract]
    public sealed class GroceryStore : Warehouse
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
        public GroceryStore(string userName)
        {
            this.userName = userName;
            group = new Group();
            cart = new Cart();
        }
        public override void BuyIteam(Goods goods, int amount)
        {
            if (cart == null)
            {
                cart = new Cart();
                cart.AddItem(goods, amount);
            }
            else
                cart.AddItem(goods, amount);
        }
        public override int GetCheck() => cart.GetCheck();
        public override void SetCatalog(List<Goods> goods) => group.SetGoods(goods);
        public void SetGoods(Goods goods) => group.AddGoods(goods);
        public override List<Goods> GetCatalog()
        {
            if (group != null)
                return group.GetGoods();
            return null;
        }
        public void AddName(string name) => this.name = name;
        public void SetDesctiption(string description) => this.description = description;
        public override string ToString() => "Bot name: " + name + " and description: " + description;
        public string[] GetCatalogInfo() => group.GetCatalog();
        public string[] GetChooseBuyer()
        {
            if (cart == null)
                return null;
            return cart.GetChoose();
        }

        public string GetName() => name;
        public string GetDescription() => description;
        public void RemoveBuyIteam() => cart = null;
    }
}
