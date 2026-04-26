using System.ComponentModel.DataAnnotations;

namespace Inventory_Managment.Models
{
    public class Item
    {
        public int Id { get; set; }

        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }

        public string? CustomId { get; set; }
        public uint xmin { get; set; }// optimistic locking
        public string? String1 { get; set; }
        public string? String2 { get; set; }
        public string? String3 { get; set; }

        public string? Text1 { get; set; }
        public string? Text2 { get; set; }
        public string? Text3 { get; set; }

        public int? Number1 { get; set; }
        public int? Number2 { get; set; }
        public int? Number3 { get; set; }

        public bool? Bool1 { get; set; }
        public bool? Bool2 { get; set; }
        public bool? Bool3 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
