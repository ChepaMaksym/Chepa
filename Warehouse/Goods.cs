using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Warehouse
{
    [DataContract]
    public class Goods
    {
        [DataMember()]
        private readonly int price;
        [DataMember()]
        private readonly string name;
        public Goods(string name, int price)
        {
            this.name = name;
            this.price = price;
        }
        public Goods(Goods goods)
        {
            name = goods.name;
            price = goods.price;
        }
        public int GetPrice()
        { 
            return price;
        }
        public string GetName()
        {
            return name;
        }
    }
}
