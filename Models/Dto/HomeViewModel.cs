namespace Inventory_Managment.Models.Dto
{
    public class HomeViewModel
    {
        public List<InventorySummaryDto> Latest { get; set; } = new();
        public List<InventorySummaryDto> TopByItems { get; set; } = new();
        public List<TagCloudItemDto> Tags { get; set; } = new();
    }

    public class InventorySummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string? CreatorName { get; set; }
        public int ItemCount { get; set; }
    }

    public class TagCloudItemDto
    {
        public string Name { get; set; } = "";
    }
}
