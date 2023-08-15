using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Warehouse
{
    [DataContract]
    public sealed class GroceryStore : Store
    {
        public GroceryStore(string userName):base(userName)
        {

        }
        public override void BuyIteam(Goods goods, int amount)
        {
            if (Cart == null)
                Cart = new Cart();
            Cart.AddItem(goods, amount);
        }
        public override int GetCheck()
        {
            if(Cart != null)
                return Cart.GetCheck();
            return 0;
        }
        public override void SetCatalog(List<Goods> goods)
        {
            if (Group != null)
                Group.SetGoods(goods);
            Group = new Group(goods);
        }
        public override List<Goods> GetCatalog()
        {
            if (Group != null)
                return Group.GetGoods();
            return null;
        }
        public void SetGoods(Goods goods)
        {
            if(Group == null)
                Group = new Group();
            Group.AddGoods(goods);
        }

        public void SetDesctiption(string description) => Description = description;
        #nullable enable
        public string[]? GetCatalogInfo()
        {
            if(Group != null)
                return Group.GetCatalog();
            return null;
        }
        #nullable enable
        public string[]? GetChooseBuyer()
        {
            if (Cart == null)
                return null;
            return Cart.GetChoose();
        }

        public void RemoveBuyIteam() => Cart = null;
        public override string ToString() => "Bot name: " + Name + " and description: " + Description;

    }
}
