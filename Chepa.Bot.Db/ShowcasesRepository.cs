using Warehouse;
namespace Chepa.Bot.Db
{
    public class ShowcasesRepository
    {
        private readonly ChepaBotContext context = new ChepaBotContext();
        public ShowcasesRepository(ChepaBotContext context)
        {
            this.context = context;
        }
        public void Add(Showcase showcase)
        {
            context.Showcases.Add(showcase);
            context.SaveChanges();
        }
        public void Update(Showcase showcase)
        {
            var existingShowcase = context.Showcases.Find(showcase.ShowcaseId);
            if (existingShowcase != null)
            {
                existingShowcase.Name = showcase.Name;
                existingShowcase.Products = showcase.Products;
                context.SaveChanges();
            }
        }
        public void AddProduct(int showcaseId, Product product)
        {
            var existingShowcase = context.Showcases.Find(showcaseId);
            if (existingShowcase != null)
            {
                existingShowcase.Products.Add(product);
                context.SaveChanges();
            }
        }
        public void UpdateProduct(int showcaseId, Product product)
        {
            var existingShowcase = context.Showcases.Find(showcaseId);
            if (existingShowcase != null)
            {
                ProductsRepository productsRepository = new ProductsRepository(context);
                productsRepository.Update(product);
                existingShowcase.Products.Add(product);
                context.SaveChanges();
            }
        }
        public void Delete(int showcaseId)
        {
            var showcaseToDelete = context.Showcases.Find(showcaseId);
            if (showcaseToDelete != null)
            {
                context.Showcases.Remove(showcaseToDelete);
                context.SaveChanges();
            }
        }
    }
}
