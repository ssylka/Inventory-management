using Inventory_Managment.Models.Directory;

namespace Inventory_Managment.Models.Dto
{
    public class InventoryDetailsViewModel
    {
        public Inventory Inventory { get; set; } = null!;
        public List<Category> Categories { get; set; } = new();
        public List<Item> Items { get; set; } = new();
        public List<CustomIdElement> CustomIdElements { get; set; } = new();
    }
}
