using Inventory_Managment.Models;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Controllers
{
    public class ItemController : Controller
    {
        private readonly AppDbContext _context;

        public ItemController(AppDbContext context)
        {
            _context = context;
        }

        // 📄 список items по inventory
        public async Task<IActionResult> Index(int inventoryId)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Fields)
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            var items = await _context.Items
                .Where(i => i.InventoryId == inventoryId)
                .ToListAsync();

            ViewBag.Inventory = inventory;

            return View(items);
        }

        // ➕ форма создания
        public IActionResult Create(int inventoryId)
        {
            ViewBag.InventoryId = inventoryId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { inventoryId = item.InventoryId });
        }
    }
}
