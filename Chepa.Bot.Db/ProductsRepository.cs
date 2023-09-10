using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Warehouse; 
namespace Chepa.Bot.Db
{
    public class ProductsRepository
    {
        private readonly ChepaBotContext context = new ChepaBotContext();

        public ProductsRepository(ChepaBotContext chepaBotContext)
        {
            context = chepaBotContext;
        }
        public IQueryable<Product> GetAll()
        {
            return context.Products;
        }

        public Product GetProduc(int id)
        {
            return context.Products.Find(id);
        }

        public void Add(Product product)
        {
            context.Products.Add(product);
            context.SaveChanges();
        }
        public void Update(Product product)
        {
            var existingProduct = context.Products.Find(product.ProductId);
            if (existingProduct != null)
            {
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var productToDelete = context.Products.Find(id);

            if (productToDelete != null)
            {
                context.Products.Remove(productToDelete);
                context.SaveChanges();
            }
        }
    }
}
