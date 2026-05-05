namespace Inventory_Managment.Models
{
    public class InventoryTag
    {
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
