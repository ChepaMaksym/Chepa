using Microsoft.EntityFrameworkCore;
using Key;
using Warehouse;
using Warehouse.Manager;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Threading;
using System;

namespace Chepa.Bot.Db
{
    public class ChepaBotContext : DbContext
    {
        public DbSet<Store> Store { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Showcase> Showcases { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<BuyItem> BuyItems { get; set; }
        public DbSet<User> Users { get; set; }
        public ChepaBotContext()
        {
            //Database.EnsureDeleted();
            //while (true)
            //    try
            //    {
            //        Database.EnsureCreated();
            //        break;
            //    }
            //    catch
            //    {
            //        System.Console.WriteLine("Bad connection");
            //    }
        }
        public ChepaBotContext(DbContextOptions<ChepaBotContext> options): base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CreatStore(modelBuilder);
            modelBuilder.Entity<Admin>();
            modelBuilder.Entity<Creator>();
            modelBuilder.Entity<Buyer>();
            modelBuilder.Entity<User>()
                .HasOne(u => u.Store)
                .WithMany(s => s.Users)
                .HasForeignKey(s => s.StoreId);
            modelBuilder.Entity<Cart>()
                .HasOne(u => u.Store)
                .WithMany()
                .HasForeignKey(s => s.StoreId);
        }
        public void CreatStore(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroceryStore>();
            modelBuilder.Entity<Store>()
                .HasMany(g => g.Showcases)
                .WithOne(s => s.Store)
                .HasForeignKey(g => g.StoreId);
            modelBuilder.Entity<Store>()
                .HasMany(s => s.Carts)
                .WithOne(c => c.Store)
                .HasForeignKey(c => c.StoreId);
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.BuyItem)
                .WithOne(b => b.Cart)
                .HasForeignKey(c => c.CartId);
            modelBuilder.Entity<Showcase>()
                .HasMany(b => b.Products)
                .WithOne(p => p.Showcase)
                .HasForeignKey(p => p.ShowcasesId);
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(PrivateKey.connectionString)
                .UseSnakeCaseNamingConvention();
        }
    }
}
