using Inventory_Managment.Models.Directory;

namespace Inventory_Managment.Models
{
    public class CustomIdElement
    {
        public int Id { get; set; }

        public int InventoryId { get; set; }

        public CustomIdElementType Type { get; set; }
        public string? Value { get; set; } 

        public int Order { get; set; } 

        public Inventory? Inventory { get; set; }
    }
}
