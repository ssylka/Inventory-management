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
                return (await GetNextSequence(inventoryId)).ToString();

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
                    Random.Shared.Next(0, 1 << 20).ToString(el.Value ?? "D6"),

                CustomIdElementType.Random32 =>
                    Random.Shared.Next().ToString(el.Value ?? "D9"),

                CustomIdElementType.Random6 =>
                    Random.Shared.Next(0, 999999).ToString(el.Value ??"D6"),

                CustomIdElementType.Random9 =>
                    Random.Shared.Next(0, 999999999).ToString(el.Value ?? "D9"),

                CustomIdElementType.Guid =>
                    Guid.NewGuid().ToString(el.Value),

                CustomIdElementType.DateTime =>
                    DateTime.UtcNow.ToString(el.Value ?? "yyyy"),

                CustomIdElementType.Sequence =>
                    (await GetNextSequence(inventoryId)).ToString(el.Value ?? "D"),

                _ => ""
            };
        }
        public async Task<int> GetNextSequence(int inventoryId)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
                throw new Exception("Inventory not found");

            inventory.LastSequence++;

            await _context.SaveChangesAsync();

            return inventory.LastSequence;
        }
    }
}
