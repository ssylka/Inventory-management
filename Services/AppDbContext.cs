using Inventory_Managment.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryField> InventoryFields { get; set; }
        public DbSet<Item> Items { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>()
                .Property<uint>("xmin")
                .IsRowVersion();

            modelBuilder.Entity<Item>()
                .HasIndex(x => new { x.CustomId, x.InventoryId })
                .IsUnique();
        }
    }
}
