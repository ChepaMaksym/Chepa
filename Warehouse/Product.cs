using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Warehouse
{
    [DataContract]
    public class Product
    {
        [DataMember()]
        private int price;
        [DataMember()]
        private string name;

        public int ProductId { get; set; }
        public int Price { get => price; set => price = value; }
        public string Name { get => name; set => name = value; }
        public int ShowcasesId { get; set; }

        public Showcase Showcase { get; set; }

        public Product()
        {

        }
        public Product(string name, int price)
        {
            this.name = name;
            this.price = price;
        }
        public Product(Product goods)
        {
            name = goods.name;
            price = goods.price;
        }
        public override string ToString()
        {
            return $"{Name} {Price}";
        }
    }
}
