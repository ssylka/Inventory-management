namespace Inventory_Managment.Models.Dto
{
    public class ProfileViewModel
    {
        public AppUser User { get; set; } = null!;
        public List<Inventory> OwnedInventories { get; set; } = new();
        public List<Inventory> AccessInventories { get; set; } = new();
        public bool IsOwnProfile { get; set; }
    }
}
