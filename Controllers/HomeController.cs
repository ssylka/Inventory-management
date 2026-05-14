using System.Diagnostics;
using Inventory_Managment.Models;
using Inventory_Managment.Models.Dto;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var isAuthenticated = User.Identity!.IsAuthenticated;

            var latest = await _context.Inventories
                .OrderByDescending(i => i.CreatedAt)
                .Take(10)
                .Select(i => new InventorySummaryDto
                {
                    Id          = i.Id,
                    Title       = i.Title,
                    Description = i.Description,
                    ImageUrl    = i.ImageUrl,
                    CreatorName = i.Creator != null ? i.Creator.Name : null,
                    ItemCount   = i.Items.Count
                })
                .ToListAsync();

            var topByItems = await _context.Inventories
                .Select(i => new InventorySummaryDto
                {
                    Id          = i.Id,
                    Title       = i.Title,
                    Description = i.Description,
                    ImageUrl    = i.ImageUrl,
                    CreatorName = i.Creator != null ? i.Creator.Name : null,
                    ItemCount   = i.Items.Count
                })
                .OrderByDescending(i => i.ItemCount)
                .Take(5)
                .ToListAsync();

            var tags = await _context.Tags
                .OrderBy(t => t.Name)
                .Select(t => new TagCloudItemDto { Name = t.Name })
                .ToListAsync();

            return View(new HomeViewModel
            {
                Latest     = latest,
                TopByItems = topByItems,
                Tags       = tags
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
