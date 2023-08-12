using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Store;
using Manager;
namespace Chepa.Bot
{
    static class FileXML
    {
        [DataMember()]
        public static List<Buyer> BuyerList { get; set; } = new List<Buyer>();
        [DataMember()]
        public static List<GroceryStore> GroceryStoreList { get; set; } = new List<GroceryStore>();
        [DataMember()]
        public static List<User> Users { get; set; } = new List<User>();

        public static List<GroceryStore> GetStoreWithNull(string userName)
        {
            GroceryStoreList = DeserializeStore();
            if (GroceryStoreList != null)
            {
                List<GroceryStore> result = new List<GroceryStore>();
                foreach (var item in GroceryStoreList)
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
                GroceryStoreList = temp;
                GroceryStore result = null;
                for (int i = 0; i < GroceryStoreList.Count; i++)
                    if (GroceryStoreList[i].UserName == userName && GroceryStoreList[i].GetDescription() == null)
                    {
                        result = GroceryStoreList[i];
                        GroceryStoreList.RemoveAt(i);
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
                GroceryStoreList = temp;
                for (int i = 0; i < GroceryStoreList.Count; i++)
                    if (GroceryStoreList[i].UserName == userName && GroceryStoreList[i].GetName() == groceryStore.GetName())
                    {
                        GroceryStoreList.RemoveAt(i);
                        GroceryStoreList.Insert(i,groceryStore);
                        SerializeStore();
                        break;
                    }
            }
        }
        public static Buyer GetBuyerWithNull(string userName, long chartID)
        {
            List<Buyer> temp = DeserializeBuyer();
            if (temp != null)
            {
                BuyerList = temp;
                foreach (var item in BuyerList)
                    if (item.GetChartID() == chartID && item.GetUserName() == userName)
                        return item;
            }
            return null;
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
        public static List<Buyer> DeserializeBuyer()
        {
            if (File.Exists("buyer.xml"))
            {
                FileStream fs = new FileStream("buyer.xml", FileMode.Open);
                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer ser = new DataContractSerializer(typeof(List<Buyer>));

                // Deserialize the data and read it from the instance.
                List<Buyer> deserializedBuyer =
                    (List<Buyer>)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                return deserializedBuyer;
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
                        return item;
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
                DataContractSerializer ser = new DataContractSerializer(typeof(List<User>));

                // Deserialize the data and read it from the instance.
                Users =
                    (List<User>)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                for (int i = 0; i < Users.Count; i++)
                    if (Users[i].GetChartID() == user.GetChartID() && Users[i].GetUserName() == user.GetUserName())
                    {
                        Users.RemoveAt(i);
                        Users.Insert(i,user);
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
        public static void SetBuyer(Buyer buyer)
        {
            if (File.Exists("buyer.xml"))
            {
                FileStream fs = new FileStream("buyer.xml", FileMode.Open);
                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
                DataContractSerializer ser = new DataContractSerializer(typeof(List<Buyer>));

                // Deserialize the data and read it from the instance.
                BuyerList =
                    (List<Buyer>)ser.ReadObject(reader, true);
                reader.Close();
                fs.Close();
                for (int i = 0; i < BuyerList.Count; i++)
                    if (BuyerList[i].GetChartID() == buyer.GetChartID() && BuyerList[i].GetUserName() == buyer.GetUserName())
                    {
                        BuyerList.RemoveAt(i);
                        BuyerList.Insert(i,buyer);
                        SerializeBuyer();
                        break;
                    }
            }
            else
            {
                BuyerList.Add(buyer);
                SerializeBuyer();
            }
        }
        public static void SerializeStore()
        {
            FileStream writer = new FileStream("store.xml", FileMode.OpenOrCreate);
            DataContractSerializer ser =
                new DataContractSerializer(typeof(List<GroceryStore>));
            ser.WriteObject(writer, GroceryStoreList);
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
        public static void SerializeBuyer()
        {
            FileStream writer = new FileStream("buyer.xml", FileMode.Create);
            DataContractSerializer ser =
                new DataContractSerializer(typeof(List<Buyer>));
            ser.WriteObject(writer, BuyerList);
            writer.Close();
        }
    }
}
