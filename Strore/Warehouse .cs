using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Store
{
    [DataContract]
    public abstract class Warehouse
    {
        public abstract void SetCatalog(List<Goods> goods);
        public abstract void BuyIteam(Goods goods, int amount);
        public abstract int GetCheck();
        public abstract List<Goods> GetCatalog();
    }
}
