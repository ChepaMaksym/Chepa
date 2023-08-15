using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.Serialization;

namespace Warehouse
{
    [DataContract]
    public class Group
    {
        [DataMember()]
        private List<Goods> goods;
        public Group()
        {
        }
        public Group(List<Goods> goods)
        {
            this.goods = goods;
        }
        public void AddGoods(Goods currentGoods)
        {
            if (goods == null)
                goods = new List<Goods>();
            goods.Add(currentGoods);
        }
        public void SetGoods(List<Goods> goods) => this.goods = goods;
        public List<Goods> GetGoods () => goods;
        public void RemoveGoods (Goods goods)
        {
            this.goods.Remove(goods);
        }
        public bool ContainGoods (Goods goods)
        {
            return this.goods.Contains(goods);
        }
        public string[] GetCatalog()
        {
            if (goods != null)
            {
                string[] text = new string[goods.Count];
                for (int i = 0; i < goods.Count; i++)
                    text[i] = $"{goods[i].GetName()} {goods[i].GetPrice()}";
                return text;
            }
            return null;
        }
    }
}
