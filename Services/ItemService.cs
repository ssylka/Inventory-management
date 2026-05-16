using Inventory_Managment.Models;
using Inventory_Managment.Models.Directory;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
        // Formats a number using pattern [prefix][D|X][width][suffix], e.g. "-D3" to "-042", "X5_" to "1A3F0_"
        private static string FormatNumber(long num, string? format)
        {
            if (string.IsNullOrEmpty(format))
                return num.ToString();

            var match = Regex.Match(format, @"^(.*?)(D|X)(\d*)(.*?)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                return num.ToString();

            var prefix = match.Groups[1].Value;
            var spec   = match.Groups[2].Value.ToUpper();
            var width  = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : int.Parse(match.Groups[3].Value);
            var suffix = match.Groups[4].Value;

            var numStr = spec == "X"
                ? num.ToString("X").PadLeft(width, '0')
                : num.ToString().PadLeft(width, '0');

            return prefix + numStr + suffix;
        }

        private async Task<string> GeneratePart(CustomIdElement el, int inventoryId)
        {
            return el.Type switch
            {
                CustomIdElementType.Fixed =>
                    el.Value ?? "",

                CustomIdElementType.Random6 =>
                    FormatNumber(Random.Shared.Next(0, 1_000_000), el.Value),

                CustomIdElementType.Random9 =>
                    FormatNumber(Random.Shared.Next(0, 1_000_000_000), el.Value),

                CustomIdElementType.Random20 =>
                    FormatNumber(Random.Shared.Next(0, 1 << 20), el.Value),

                CustomIdElementType.Random32 =>
                    FormatNumber(Random.Shared.NextInt64(0, 1L << 32), el.Value),

                CustomIdElementType.Guid =>
                    Guid.NewGuid().ToString(el.Value),

                CustomIdElementType.DateTime =>
                    DateTime.UtcNow.ToString(el.Value ?? "yyyy"),

                CustomIdElementType.Sequence =>
                    FormatNumber(await GetNextSequence(inventoryId), el.Value),

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
            await _context.SaveChangesAsync(); // commit before item insert — survives rollback of the item

            return inventory.LastSequence;
        }
    }
}
