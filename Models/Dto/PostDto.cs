namespace Inventory_Managment.Models.Dto
{
    public class PostDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public string UserId { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public bool LikedByMe { get; set; }
    }
}
