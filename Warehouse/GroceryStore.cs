using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Warehouse
{
    [DataContract]
    public sealed class GroceryStore : Store
    {
        public GroceryStore()
        {

        }
        public GroceryStore(string userName):base(userName)
        {

        }
        public override void BuyIteam(Cart cartWithStoreUserId)
        {
            basket.Add(cartWithStoreUserId);
        }
        public override int GetCheck()//make
        {
            int check = 0;
            if (Carts != null)
                foreach (var item in Carts)
                {
                    check += item.GetCheck();
                }
            return check;
        }
        public override void SetCatalog(List<Product> goods,string nameShowcase)
        {
            if (goodsCatalog == null)
                goodsCatalog = new List<Showcase>();
            goodsCatalog.Add(new Showcase() { Products = goods, Name = nameShowcase });
        }
        public override List<Product> GetCatalog(string nameShowcase)
        {
            if (goodsCatalog != null)
                return goodsCatalog
                    .Where(s => s.Name == nameShowcase)
                    .SelectMany(s => s.Products)
                    .ToList();
            return null;
        }
        public void AddProduct(Product product, string nameShowcase)
        {
            if (goodsCatalog != null)
            {
                var targetShowcase = goodsCatalog.FirstOrDefault(showcase => showcase.Name == nameShowcase);
                if (targetShowcase != null)
                    targetShowcase.Products.Add(product);
                else
                    goodsCatalog.Add(new Showcase() { Name = nameShowcase, Products = new List<Product>() { new Product(product) } });
            }
            else
                goodsCatalog = new List<Showcase>() { new Showcase() { Name = nameShowcase, Products = new List<Product>() { new Product(product) } } };
        }

        public void SetDesctiption(string description) => Description = description;
#nullable enable
        public string[]? GetCatalog()
        {
            if (goodsCatalog != null)
            {
                string[] text = new string[goodsCatalog.Count];
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < goodsCatalog.Count; i++)
                {
                    stringBuilder.AppendLine(goodsCatalog[i].Name);
                    stringBuilder.Append(goodsCatalog[i].GetCatalog());
                    text[i] += stringBuilder;
                }
                return text;
            }
            return null;
        }
        public string[]? GetProducts(string nameShowcase)
        {
            if (goodsCatalog != null)
            {
                Showcase showcase = Showcases.FirstOrDefault(showcase => showcase.Name == nameShowcase);
                if (showcase != null)
                {
                    string[] text = new string[showcase.Products.Count];
                    for (int i = 0; i < showcase.Products.Count; i++)
                        text[i] = $"{showcase.Products[i].Name} {showcase.Products[i].Price}";
                    return text;
                }
            }
            return null;
        }
#nullable enable
        public override List<string>? GetChooseBuyer()
        {
            if (Carts == null)
                return null;
            List<string> text = new List<string>();
            for (int i = 0; i < Carts.Count; i++)
            {
                if (Carts[i].IsNewBuyIteamsFromBuyer == true)
                {
                    text.AddRange(Carts[i].GetChoose());
                }
            }
            return text;
        }

        public override void RemoveCart(int cardId) => Carts.RemoveAll(c => c.CartId == cardId);
        public override string ToString() => "Bot name: " + Name + " and description: " + Description;

    }
}
