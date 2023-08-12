using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Store;
using Manager;
namespace Manager
{
    public static class FileXML
    {
        [DataMember()]
        public static List<GroceryStore> Store { get; set; } = new List<GroceryStore>();
        [DataMember()]
        public static List<User> Users { get; set; } = new List<User>();

        public static List<GroceryStore> GetStoreWithNull(string userName)
        {
            Store = DeserializeStore();
            if (Store != null)
            {
                List<GroceryStore> result = new List<GroceryStore>();
                foreach (var item in Store)
                    if (item.UserName == userName)
                        result.Add(item);
                if (result.Count != 0)
                    return result;
            }
            return null;
        }
        public static bool IsStore(string store)
        {
            List<GroceryStore> temp = DeserializeStore();
            if (temp != null)
            {
                foreach (var item in temp)
                    if (item.GetName() == store)
                        return true;
            }
            return false;
        }
        public static GroceryStore GetCreatedStore(string userName)
        {
            List<GroceryStore> temp = DeserializeStore();
            if (temp != null)
            {
                Store = temp;
                GroceryStore result = null;
                for (int i = 0; i < Store.Count; i++)
                    if (Store[i].UserName == userName && Store[i].GetDescription() == null)
                    {
                        result = Store[i];
                        Store.RemoveAt(i);
                        break;
                    }
                if (result != null)
                    return result;
            }
            return null;
        }
        public static void AddCatalogStore(string userName, GroceryStore groceryStore)
        {
            List<GroceryStore> temp = DeserializeStore();
            if (temp != null)
            {
                Store = temp;
                for (int i = 0; i < Store.Count; i++)
                    if (Store[i].UserName == userName && Store[i].GetName() == groceryStore.GetName())
                    {
                        Store.RemoveAt(i);
                        Store.Insert(i, groceryStore);
                        SerializeStore();
                        break;
                    }
            }
        }
        public static GroceryStore GetStore(int index)
        {
            return DeserializeStore()[index];
        }

        public static List<GroceryStore> DeserializeStore()
        {
            if (File.Exists("store.xml"))
            {
                FileStream fs = new FileStream("store.xml", FileMode.Open);
                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer ser = new DataContractSerializer(typeof(List<GroceryStore>));

                // Deserialize the data and read it from the instance.
                List<GroceryStore> deserializedStore =
                    (List<GroceryStore>)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                return deserializedStore;
            }
            else
                return null;
        }
        public static User GetUserWithNull(string userName, long chartID)
        {
            if (File.Exists("user.xml"))
            {
                FileStream fs = new FileStream("user.xml", FileMode.Open);
                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer ser = new DataContractSerializer(typeof(List<User>));

                // Deserialize the data and read it from the instance.
                List<User> deserializedUsers =
                    (List<User>)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                foreach (var item in deserializedUsers)
                    if (item.GetChartID() == chartID && item.GetUserName() == userName)
                    {
                        switch (item.GetRights())
                        {
                            case Rights.Watcher:
                                return item;
                            case Rights.Buyer:
                                {
                                    Buyer buyer = (Buyer)item;
                                    return buyer;
                                }
                            case Rights.CreatorBot:
                                {
                                    Creator creator = (Creator)item;
                                    return creator;
                                }
                            case Rights.Admin:
                                {
                                    Admin admin = (Admin)item;
                                    return admin;
                                }
                            default:
                                return item;
                        }

                    }
                return null;
            }
            else
                return null;
        }
        public static void SetUser(User user)
        {
            if (File.Exists("user.xml"))
            {
                FileStream fs = new FileStream("user.xml", FileMode.Open);
                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer serUser = new DataContractSerializer(typeof(List<User>));

                // Deserialize the data and read it from the instance.
                if (user is User)
                    Users = (List<User>)serUser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                for (int i = 0; i < Users.Count; i++)
                    if (Users[i].GetChartID() == user.GetChartID() && Users[i].GetUserName() == user.GetUserName())
                    {
                        Users.RemoveAt(i);
                        if (user.GetRights() == Rights.Admin)
                            Users.Insert(i, (Admin)user);
                        else if (user.GetRights() == Rights.Buyer)
                            Users.Insert(i, (Buyer)user);
                        else if (user.GetRights() == Rights.CreatorBot)
                            Users.Insert(i, (Creator)user);
                        else
                            Users.Insert(i, user);
                        SerializeUser();
                        break;
                    }
            }
            else
            {
                Users.Add(user);
                SerializeUser();
            }
        }
        public static void SetStore(GroceryStore store, Creator creator)
        {
            if (File.Exists("store.xml"))
            {
                FileStream fs = new FileStream("store.xml", FileMode.Open);
                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer serStore = new DataContractSerializer(typeof(List<GroceryStore>));

                Store = (List<GroceryStore>)serStore.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                for (int i = 0; i < Store.Count; i++)
                    if (Store[i].UserName == creator.GetUserName() && Store[i].GetName() == store.GetName())
                    {
                        Store.RemoveAt(i);
                        Store.Insert(i, store);
                        SerializeStore();
                        break;
                    }
            }
            else
            {
                Store.Add(store);
                SerializeStore();
            }
        }
        public static void SerializeStore()
        {
            FileStream writer = new FileStream("store.xml", FileMode.OpenOrCreate);
            DataContractSerializer ser =
                new DataContractSerializer(typeof(List<GroceryStore>));
            ser.WriteObject(writer, Store);
            writer.Close();
        }
        public static void SerializeUser()
        {
            FileStream writer = new FileStream("user.xml", FileMode.Create);
            DataContractSerializer ser =
                new DataContractSerializer(typeof(List<User>));
            ser.WriteObject(writer, Users);
            writer.Close();
        }
    }
}
