namespace Inventory_Managment.Models.Dto
{
    public class UserAdminDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsAdmin { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsUnverified { get; set; }
    }
}
