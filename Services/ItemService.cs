using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Services
{
    public class ItemService
    {
        AppDbContext _context;

        public ItemService(AppDbContext appDbContext) => _context = appDbContext;

        public async Task<string> GenerateCustomIdAsync(int inventoryId)
        {
            var lastItem = await _context.Items
                .Where(i => i.InventoryId == inventoryId && i.CustomId != null)
                .OrderByDescending(i => i.CustomId)
                .FirstOrDefaultAsync();

            int next = 1;

            if (lastItem != null)
            {
                var parts = lastItem.CustomId.Split('-');
                if (int.TryParse(parts.Last(), out int lastNumber))
                {
                    next = lastNumber + 1;
                }
            }

            return $"ITEM-{DateTime.UtcNow.Year}-{next:D4}";
        }
    }
}
