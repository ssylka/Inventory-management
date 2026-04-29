using Microsoft.AspNetCore.Identity;

namespace Inventory_Managment.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
