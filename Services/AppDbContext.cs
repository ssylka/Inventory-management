using Inventory_Managment.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Services
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole, string>
    {
        public DbSet<CustomIdElement> CustomIdElements { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryField> InventoryFields { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<InventoryAccess> InventoryAccess { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>()
                .Property<uint>("xmin")
                .IsRowVersion();

            modelBuilder.Entity<Inventory>()
                .Property<uint>("xmin")
                .IsRowVersion();

            modelBuilder.Entity<Item>()
                .HasIndex(x => new { x.CustomId, x.InventoryId })
                .IsUnique();
        }
    }
}
