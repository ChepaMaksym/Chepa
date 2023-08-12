using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Store
{
    [DataContract]
    class BuyItem
    {
        [DataMember()]
        private readonly Goods goods;
        [DataMember()]
        private readonly int amount;
        public BuyItem(Goods goods, int amount = 1)
        {
            this.goods = new Goods(goods);
            this.amount = amount;
        }
        public int GetPrice()
        {
            return goods.GetPrice() * amount;
        }
        public Goods GetGoods()
        {
            return goods;
        }
    }
}
