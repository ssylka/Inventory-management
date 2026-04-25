using Inventory_Managment.Models;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Controllers
{
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;

        public InventoryController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var inventories = await _context.Inventories.ToListAsync();
            return View(inventories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Inventory inventory)
        {
            if (!ModelState.IsValid)
                return View(inventory);

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            var items = await _context.Inventories
                .Where(i => ids.Contains(i.Id))
                .ToListAsync();

            if (items.Count != ids.Count)
                return Conflict("Some inventories no longer exist.\nPlease try again.");

            _context.Inventories.RemoveRange(items);
            await _context.SaveChangesAsync();

            return Ok();
        }
        public async Task<IActionResult> Fields(int id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == id);

            return View(inventory);
        }
        public IActionResult AddField(int id)
        {
            ViewBag.InventoryId = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddField(InventoryField field)
        {
            field.Id = 0; // Ensure EF Core treats this as a new entity
            field.Slot = GetNextSlot(field.Type, field.InventoryId);

            _context.InventoryFields.Add(field);
            await _context.SaveChangesAsync();

            return RedirectToAction("Fields", new { id = field.InventoryId });
        }
        private string GetNextSlot(FieldType type, int inventoryId)
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

            throw new Exception("Max fields reached");
        }
    }
}
