namespace Inventory_Managment.Models.Dto
{
    public class InventoryEditDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int CategoryId { get; set; }
        public bool IsPublic { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> TagNames { get; set; } = new();
        public uint xmin { get; set; }
        public List<CustomIdElementDto> CustomIdElements { get; set; } = new();
        public List<FieldOrderDto> FieldOrders { get; set; } = new();
    }
}
