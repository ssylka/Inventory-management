using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Services
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }
}
