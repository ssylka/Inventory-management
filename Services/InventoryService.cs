using Inventory_Managment.Models;
using Inventory_Managment.Models.Directory;
using Inventory_Managment.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Services
{
    public class InventoryService
    {
        private readonly AppDbContext _context;
        
        public InventoryService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<string> GetNextSlotAsync(FieldType type, int inventoryId)
        {
            var existing = await _context.InventoryFields
                .Where(f => f.InventoryId == inventoryId && f.Type == type)
                .Select(f => f.Slot)
                .ToListAsync();

            var prefix = type.ToString();

            for (int i = 1; i <= 3; i++)
            {
                var slot = $"{prefix}{i}";
                if (!existing.Contains(slot))
                    return slot;
            }

            throw new Exception("You cannot add more than 3 fields of this type.");
        }
        // Can add/edit/delete items: admin, creator, explicit access, or any active user if public
        public bool CanEdit(Inventory inv, string? userId, bool isAdmin, bool isActive)
        {
            if (isAdmin) return true;
            if (!isActive || string.IsNullOrEmpty(userId)) return false;
            if (inv.CreatorId == userId) return true;
            if (inv.IsPublic) return true;
            return _context.InventoryAccess
                .Any(a => a.InventoryId == inv.Id && a.UserId == userId);
        }

        // Can edit inventory settings/fields/custom-id: admin or creator only
        public bool CanEditSettings(Inventory inv, string? userId, bool isAdmin)
        {
            if (isAdmin) return true;
            return !string.IsNullOrEmpty(userId) && inv.CreatorId == userId;
        }
        public async Task UpdateTagsForInventoryAsync(Inventory inventory)
        {
            // diff update
            var existing = await _context.InventoryTags
                .Include(x => x.Tag)
                .Where(x => x.InventoryId == inventory.Id && x.Tag != null)
                .ToListAsync();

            var existingNames = existing
                .Select(x => x.Tag.Name)
                .ToHashSet();

            var newNames = inventory.TagNames
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .ToHashSet();

            var toRemove = existing
                .Where(x => !newNames.Contains(x.Tag.Name))
                .ToList();

            _context.InventoryTags.RemoveRange(toRemove);

            var toAdd = newNames
                .Where(n => !existingNames.Contains(n) 
                            && !string.IsNullOrWhiteSpace(n))
                .Select(t => t.Trim())
                .Distinct()
                .ToList();

            foreach (var tagName in toAdd)
            {
                var tag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name == tagName);

                if (tag == null)
                {
                    tag = new Tag { Name = tagName };
                    _context.Tags.Add(tag);
                }

                _context.InventoryTags.Add(new InventoryTag
                {
                    InventoryId = inventory.Id,
                    Tag = tag
                });
            }
            await _context.SaveChangesAsync();
        }
    }
}
