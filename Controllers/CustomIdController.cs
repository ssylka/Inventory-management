using Inventory_Managment.Models;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Controllers
{
    public class CustomIdController : Controller
    {
        private readonly AppDbContext _context;

        public CustomIdController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Edit(int inventoryId)
        {
            var elements = await _context.CustomIdElements
                .Where(e => e.InventoryId == inventoryId)
                .OrderBy(e => e.Order)
                .ToListAsync();

            ViewBag.InventoryId = inventoryId;
            return View(elements);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(List<CustomIdElement> elements, int inventoryId)
        {
            var old = _context.CustomIdElements
                .Where(e => e.InventoryId == inventoryId);

            _context.CustomIdElements.RemoveRange(old);

            foreach (var el in elements)
            {
                el.InventoryId = inventoryId;
            }

            _context.CustomIdElements.AddRange(elements);

            await _context.SaveChangesAsync();

            return Redirect($"/Inventory/Details/{inventoryId}#custom-id");
        }
    }
}
