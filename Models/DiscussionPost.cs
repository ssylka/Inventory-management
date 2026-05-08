namespace Inventory_Managment.Models
{
    public class DiscussionPost
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; } = null!;
        public string UserId { get; set; } = "";
        public AppUser User { get; set; } = null!;
        public string Text { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public List<PostLike> Likes { get; set; } = new();
    }
}
