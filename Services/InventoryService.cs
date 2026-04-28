using Inventory_Managment.Models;
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
        public string GetNextSlot(FieldType type, int inventoryId)
        {
            var existing = _context.InventoryFields
                .Where(f => f.InventoryId == inventoryId && f.Type == type)
                .Select(f => f.Slot)
                .ToList();

            var prefix = type.ToString();

            for (int i = 1; i <= 3; i++)
            {
                var slot = $"{prefix}{i}";
                if (!existing.Contains(slot))
                    return slot;
            }

            throw new Exception("You cannot add more than 3 fields of this type.");
        }
        public bool CanEdit(Inventory inv, string userId, bool isAdmin)
        {
            if (isAdmin) return true;
            if (inv.CreatorId == userId) return true;
            if (inv.IsPublic) return true;

            return _context.InventoryAccess
                .Any(a => a.InventoryId == inv.Id && a.UserId == userId);
        }
    }
}
