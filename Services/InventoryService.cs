using Inventory_Managment.Models;
using Inventory_Managment.Models.Directory;
using Inventory_Managment.Models.Dto;
using Microsoft.AspNetCore.Identity;
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

        public async Task<bool> CanEditAsync(Inventory inv, string? userId, bool isAdmin, bool isActive)
        {
            if (isAdmin) return true;
            if (!isActive || string.IsNullOrEmpty(userId)) return false;
            if (inv.CreatorId == userId) return true;
            if (inv.IsPublic) return true;
            return await _context.InventoryAccess
                .AnyAsync(a => a.InventoryId == inv.Id && a.UserId == userId);
        }

        public bool CanEditSettings(Inventory inv, string? userId, bool isAdmin)
        {
            if (isAdmin) return true;
            return !string.IsNullOrEmpty(userId) && inv.CreatorId == userId;
        }

        public async Task ReplaceCustomIdElementsAsync(int inventoryId, IList<CustomIdElementDto> elements)
        {
            var old = await _context.CustomIdElements
                .Where(e => e.InventoryId == inventoryId)
                .ToListAsync();
            _context.CustomIdElements.RemoveRange(old);
            _context.CustomIdElements.AddRange(elements.Select(e => new CustomIdElement
            {
                InventoryId = inventoryId,
                Type        = Enum.Parse<CustomIdElementType>(e.Type),
                Value       = string.IsNullOrEmpty(e.Value) ? null : e.Value,
                Order       = e.Order
            }));
        }

        public IQueryable<Inventory> GetSearchedInventoris(string searchText, bool isAuthenticated, bool isAdmin, string? userId)
        {
            var inventory = _context.Inventories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                inventory = inventory.Include(i => i.Fields).Include(i => i.Items).Include(i => i.AccessList)
                                                       // inventory's fields' name and description
                                                       .Where(i => i.Fields.Any(f => (f.Name.ToLower().Contains(searchText)) ||
                                                            string.IsNullOrWhiteSpace(i.Description) ? i.Description.ToLower().Contains(searchText) : false)
                                                       // inventory's items' string and text fields
                                                       || ((i.Items.Any(it => (it.String1 != null && it.String1.ToLower().Contains(searchText)) ||
                                                            (it.String2 != null && it.String2.ToLower().Contains(searchText)) ||
                                                            (it.String3 != null && it.String3.ToLower().Contains(searchText)) ||
                                                            (it.Text1 != null && it.Text1.ToLower().Contains(searchText)) ||
                                                            (it.Text2 != null && it.Text2.ToLower().Contains(searchText)) ||
                                                            (it.Text3 != null && it.Text3.ToLower().Contains(searchText)))))
                                                       // inventory's title and description
                                                       || (i.Title.ToLower().Contains(searchText) || i.Description.ToLower().Contains(searchText)))
                                                       .OrderByDescending(i => i.Title);
                return inventory;
            }
            else 
                throw new Exception("Search text cannot be empty.");
        }


        public async Task UpdateAccessAsync(int inventoryId, IList<string> userIds)
        {
            var existing = await _context.InventoryAccess
                .Where(a => a.InventoryId == inventoryId)
                .ToListAsync();

            var existingIds = existing.Select(a => a.UserId).ToHashSet();

            var toRemove = existing.Where(a => !userIds.Contains(a.UserId)).ToList();
            var toAdd    = userIds
                .Where(uid => !existingIds.Contains(uid))
                .Select(uid => new InventoryAccess { InventoryId = inventoryId, UserId = uid });

            _context.InventoryAccess.RemoveRange(toRemove);
            _context.InventoryAccess.AddRange(toAdd);
        }

        public async Task ApplyFieldOrdersAsync(IList<FieldOrderDto> orders)
        {
            if (!orders.Any()) return;

            var ids    = orders.Select(f => f.Id).ToList();
            var fields = await _context.InventoryFields
                .Where(f => ids.Contains(f.Id))
                .ToListAsync();

            foreach (var fo in orders)
            {
                var field = fields.FirstOrDefault(f => f.Id == fo.Id);
                if (field != null) field.Order = fo.Order;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateTagsForInventoryAsync(Inventory inventory)
        {
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
                .Where(n => !existingNames.Contains(n) && !string.IsNullOrWhiteSpace(n))
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
