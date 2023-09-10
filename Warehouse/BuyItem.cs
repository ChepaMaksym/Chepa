using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Warehouse
{
    [DataContract]
    public class BuyItem
    {
        [DataMember()]
        private Product product = new Product();
        [DataMember()]
        private int amount;
        public int BuyItemId { get; set; }
        public int Amount { get => amount; set => amount = value; }
        public string Name { get => product.Name; set => product.Name = value; }
        public int Price { get => product.Price; set => product.Price = value; }

        public int CartId { get; set; }

        public Cart Cart { get; set; }

        public BuyItem()
        {

        }
        public BuyItem(Product product, int amount = 1)
        {
            this.product = product;
            this.amount = amount;
        }
        public int GetPrice()
        {
            return product.Price * amount;
        }
        public Product GetGoods()
        {
            return product;
        }
        public override string ToString()
        {
            return $"Id: {product.ProductId} name: {product.Name} price: {product.Price} amount: {amount}";
        }
    }
}
