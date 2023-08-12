using System;
using System.Runtime.Serialization;
namespace Manager
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
    [KnownType(typeof(Admin))]
    [KnownType(typeof(Buyer))]
    [KnownType(typeof(Creator))]
    [DataContract]
    public class User
    {
        [DataMember]
        private readonly string userName;
        [DataMember]
        private readonly long chartID;
        [DataMember]
        private Rights rights;
        [DataMember]
        private int indexStore = -1;
        [DataMember]
        public bool isSetBuyItem { get; set; }

        public User(string userName, long chartID)
        {
            this.userName = userName;
            this.chartID = chartID;
            rights = Rights.Watcher;
        }
        public long GetChartID()
        {
            return chartID;
        }
        public Rights GetRights()
        {
            return rights;
        }
        public void SetRights(Rights rights)
        {
             this.rights = rights;
        }
        public string GetUserName()
        {
            return userName;
        }
        public void SetIndexStore(int indexStore)
        {
            this.indexStore = indexStore;
        }
        public int GetIndexStore()
        {
            return indexStore;
        }

    }
}
