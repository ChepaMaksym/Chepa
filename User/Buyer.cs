using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Warehouse;

namespace Warehouse.Manager
{
    [DataContract,KnownType(typeof(User))]
    public sealed class Buyer : User
    {
        public Buyer():base()
        {

        }
        public Buyer(User user) : base(user)
        {
            Rights = Rights.Buyer;
        }
        public Buyer(string userName, long chartID, int userId):base(userName,chartID)
        {
            Rights = Rights.Buyer;
            UserId = userId;
        }
        public void ChooseIteams(Cart cartWithStoreUserId)
        {
            Store.BuyIteam(cartWithStoreUserId);
        }
        public Store GetStore()
        {
            return Store;
        }
        public List<string>? GetChoose()
        {
            if(Store != null)
                return Store.GetChooseBuyer();
            return null;
        }
        public int GetCheck()
        {
            return Store.GetCheck();
        }
        public void RemoveBuyIteam(int cartId) => Store.RemoveCart(cartId);
    }
}
