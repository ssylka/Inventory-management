using Inventory_Managment.Models;
using Inventory_Managment.Models.Directory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Inventory_Managment.Services
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole, string>
    {
        public DbSet<CustomIdElement> CustomIdElements { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryField> InventoryFields { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<InventoryAccess> InventoryAccess { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<InventoryTag> InventoryTags { get; set; }
        public DbSet<DiscussionPost> DiscussionPosts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InventoryTag>()
                .HasKey(x => new { x.InventoryId, x.TagId });

            modelBuilder.Entity<InventoryTag>()
                .HasOne(x => x.Inventory)
                .WithMany(i => i.InventoryTags)
                .HasForeignKey(x => x.InventoryId);

            modelBuilder.Entity<InventoryTag>()
                .HasOne(x => x.Tag)
                .WithMany(t => t.InventoryTags)
                .HasForeignKey(x => x.TagId);

            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<Category>().ToTable("Category");

            modelBuilder.Entity<Item>()
                .Property<uint>("xmin")
                .IsRowVersion();

            modelBuilder.Entity<Inventory>()
                .Property<uint>("xmin")
                .IsRowVersion();

            modelBuilder.Entity<InventoryField>()
                .Property<uint>("xmin")
                .IsRowVersion();

            modelBuilder.Entity<Item>()
                .HasIndex(x => new { x.CustomId, x.InventoryId })
                .IsUnique();

            modelBuilder.Entity<PostLike>()
                .HasIndex(l => new { l.PostId, l.UserId })
                .IsUnique();

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Equipment" },
                new Category { Id = 2, Name = "Furniture" },
                new Category { Id = 3, Name = "Book" },
                new Category { Id = 4, Name = "Other" }
            );
        }
    }
}
