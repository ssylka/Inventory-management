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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<InventoryField> Fields { get; set; } = new();
        public List<Item> Items { get; set; } = new();
    }
}
