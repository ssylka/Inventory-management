namespace Inventory_Managment.Models
{
    public class PostLike
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public DiscussionPost Post { get; set; } = null!;
        public string UserId { get; set; } = "";
        public AppUser User { get; set; } = null!;
    }
}
