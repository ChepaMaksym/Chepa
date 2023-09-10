using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Warehouse
{
    [DataContract]
    public class Showcase
    {
        [DataMember()]
        private List<Product> product;
        [Key]
        public int ShowcaseId { get; set; }
        public string Name { get; set; }

        [ForeignKey(nameof(Store))]
        public int StoreId { get; set; }

        public Store Store { get; set; }
        public List<Product> Products { get => product; set => product = value; }

        public Showcase()
        {
        }
        public void AddGoods(Product currentGoods)
        {
            if (product == null)
                product = new List<Product>();
            product.Add(currentGoods);
        }
        public void SetGoods(List<Product> goods) => this.product = goods;
        public List<Product> GetGoods () => product;
        public void RemoveGoods (Product goods)
        {
            this.product.Remove(goods);
        }
        public bool ContainGoods (Product goods)
        {
            return this.product.Contains(goods);
        }
        public StringBuilder? GetCatalog()
        {
            if (product != null)
            {
                StringBuilder text = new StringBuilder(product.Count);
                foreach (var item in product)
                {
                    text.AppendLine($"{item.Name} {item.Price}");
                }
                return text;
            }
            return null;
        }
        public override string ToString()
        {
            string text = null;
            foreach (var item in product)
            {
                text += item.ToString();
                text += '\n';
            }
            return text;
        }
    }
}
