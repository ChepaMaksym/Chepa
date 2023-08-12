using System;
using System.Runtime.Serialization;
using Store;

namespace Manager
{
    [DataContract,KnownType(typeof(User))]
    public sealed class Buyer : User
    {
        [DataMember()]
        private GroceryStore store;//make set or change
        public Buyer(string userName, long chartID):base(userName,chartID)
        {
            SetRights(Rights.Buyer);
        }
        public void ChooseIteams(Goods goods, int amount = 1)
        {
            store.BuyIteam(goods, amount);
        }
        public void SetStore(GroceryStore groceryStore)
        {
            store = groceryStore;
        }
        public GroceryStore GetStore()
        {
            return store;
        }
        public string[] GetChoose() => store.GetChooseBuyer();
        public int GetCheck()
        {
            return store.GetCheck();
        }
        public void RemoveBuyIteam() => store.RemoveBuyIteam();
    }
}
