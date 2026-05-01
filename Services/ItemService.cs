using Inventory_Managment.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Services
{
    public class ItemService
    {
        AppDbContext _context;

        public ItemService(AppDbContext appDbContext) => _context = appDbContext;

        public async Task<string> GenerateCustomIdAsync(int inventoryId)
        {
            var elements = await _context.CustomIdElements
                .Where(e => e.InventoryId == inventoryId)
                .OrderBy(e => e.Order)
                .ToListAsync();

            if (!elements.Any())
                return await GetNextSequence(inventoryId, null);

            var parts = new List<string>();

            foreach (var el in elements)
            {
                parts.Add(await GeneratePart(el, inventoryId));
            }

            return string.Join("", parts);
        }
        private async Task<string> GeneratePart(CustomIdElement el, int inventoryId)
        {
            return el.Type switch
            {
                CustomIdElementType.Fixed => el.Value ?? "",

                CustomIdElementType.Random20 =>
                    Random.Shared.Next(0, 1 << 20).ToString(),

                CustomIdElementType.Random32 =>
                    Random.Shared.Next().ToString(),

                CustomIdElementType.Random6 =>
                    Random.Shared.Next(0, 999999).ToString("D6"),

                CustomIdElementType.Random9 =>
                    Random.Shared.Next(0, 999999999).ToString("D9"),

                CustomIdElementType.Guid =>
                    Guid.NewGuid().ToString(),

                CustomIdElementType.DateTime =>
                    DateTime.UtcNow.ToString(el.Value ?? "yyyy"),

                CustomIdElementType.Sequence =>
                    await GetNextSequence(inventoryId, el.Value),

                _ => ""
            };
        }
        private async Task<string> GetNextSequence(int inventoryId, string? format)
        {
            var customIds = await _context.Items
                .Where(i => i.InventoryId == inventoryId && i.CustomId != null)
                .Select(i => i.CustomId!)
                .ToListAsync();

            int max = 0;

            foreach (var id in customIds)
            {
                if (int.TryParse(id, out int num))
                {
                    if (num > max)
                        max = num;
                }
            }

            var next = max + 1;

            return next.ToString(format ?? "D");
        }
    }
}
