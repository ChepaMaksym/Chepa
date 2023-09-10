using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
namespace Warehouse
{
    [DataContract]
    public class Cart
    {
        [DataMember()]
        private List<BuyItem> items = new List<BuyItem>();
        private int check;
        public int CartId { get; set; }
        public int Check { get => check; set => check = value; }

        [ForeignKey(nameof(Store))]
        public int StoreId { get; set; }
        public int UserId { get; set; }
        public bool IsNewBuyIteamsFromBuyer { get; set; }

        public Store Store { get; set; }
        public List<BuyItem> BuyItem { get => items; set => items = value; }

        public Cart()
        {
        }
        public Cart(int storeId, int userId, bool isBuyIteamsFromBuyer, Product product)
        {
            items.Add(new BuyItem(product));
            StoreId = storeId;
            IsNewBuyIteamsFromBuyer = isBuyIteamsFromBuyer;
            UserId = userId;

        }

        public int GetCheck()
        {
            int sum = 0;
            foreach (var buyItem in items)
            {
                sum += buyItem.GetPrice();
            }
            IsNewBuyIteamsFromBuyer = false;
            Check = sum;
            return sum;

        }
        public void AddItem(Product goods, int amount = 1)
        {
            BuyItem buyItem = new BuyItem(goods, amount);
            items.Add(buyItem);
        }
        public bool RemoveItem(Product goods)
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
                text[i] = $"{items[i].Name} {items[i].GetPrice()}";
            return text;
        }
        public override string ToString()//stringBuilder
        {
            string text = null;
            foreach (var item in items)
            {
                text += item.ToString();
                text += '\n';
            }
            return text;
        }
    }
}
