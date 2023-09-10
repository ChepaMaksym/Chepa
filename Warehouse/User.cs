using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
namespace Warehouse.Manager
{
    [Flags]
    public enum Rights
    {
        None = 0,
        Watcher = 1,
        Buyer = 2,
        CreatorBot = 4,
        Admin = 8,
        AllRights = Watcher | Buyer | CreatorBot | Admin
    }
    public class User
    {
        private string userName;
        private long chatID;
        private Rights rights;
        public int UserId { get; set; }
        public long ChatId { get => chatID; set => chatID = value; }
        public string UserName { get => userName; set => userName = value; }
        public Rights Rights { get => rights; set => rights = value; }
        public bool IsSetBuyItem { get; set; }

        public int? StoreId { get; set; }
        public Store Store { get; set; }

        public User() { }

        public User(string UserName, long ChartID)
        {
            userName = UserName;
            chatID = ChartID;
            Rights = Rights.Watcher;
        }
        public User(User user)
        {
            UserId = user.UserId;
            userName = user.UserName;
            chatID = user.ChatId;
            Rights = user.Rights;
            StoreId = user.StoreId;
            IsSetBuyItem = user.IsSetBuyItem;
            Store = user.Store;
        }
    }
}
