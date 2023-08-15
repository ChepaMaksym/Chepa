using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Warehouse
{
    [DataContract]
    public class Cart
    {
        [DataMember()]
        private readonly List<BuyItem> items;
        public Cart()
        {
            items = new List<BuyItem>();
        }
        public int GetCheck()
        {
            int sum = 0;
            foreach (var a in items)
                sum += a.GetPrice();
            return sum;

        }
        public void AddItem(Goods goods, int amount = 1)
        {
            BuyItem buyItem = new BuyItem(goods, amount);
            items.Add(buyItem);
        }
        public bool RemoveItem(Goods goods)
        {
            var buyItem = items.SingleOrDefault(r => r.GetGoods() == goods);
            if (buyItem != null)
            {
                items.Remove(buyItem);
                return true;
            }
            return false;
        }
        public string[] GetChoose()
        {
            string[] text = new string[items.Count];
            for (int i = 0; i < items.Count; i++)
                text[i] = $"{items[i].GetGoods().GetName()} {items[i].GetPrice()}";
            return text;
        }
    }
}
