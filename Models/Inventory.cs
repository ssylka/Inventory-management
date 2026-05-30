using Inventory_Managment.Models.Directory;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Managment.Models
{
    public class Inventory
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }
        public string? CreatorId { get; set; }
        public AppUser? Creator { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<InventoryField> Fields { get; set; } = new();
        public List<Item> Items { get; set; } = new();
        public bool IsPublic { get; set; } = false;
        public int LastSequence { get; set; }
        public uint xmin { get; set; } // optimistic locking
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? ImageUrl { get; set; }
        public List<InventoryTag> InventoryTags { get; set; } = new();
        public List<InventoryAccess> AccessList { get; set; } = new();

        /// <summary>Per-inventory API token for external read-only access.</summary>
        public string? ApiToken { get; set; }
    }
}
