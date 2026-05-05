namespace Inventory_Managment.Models.Dto
{
    public class InventoryEditDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int CategoryId { get; set; }
        public bool IsPublic { get; set; }
        public string? ImageUrl { get; set; }

        public string Tags { get; set; } = "";

        public uint xmin { get; set; }
    }
}
