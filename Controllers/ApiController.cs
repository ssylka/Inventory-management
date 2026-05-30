using Inventory_Managment.Models.Directory;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Controllers
{
    /// <summary>
    /// Read-only REST API secured by a per-inventory API token.
    /// GET /api/inventory?token=&lt;token&gt;
    /// </summary>
    [Route("api/inventory")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly StatService _statService;

        public ApiController(AppDbContext context, StatService statService)
        {
            _context = context;
            _statService = statService;
        }

        // GET /api/inventory?token=<token>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized(new { error = "API token is required." });

            var inventory = await _context.Inventories
                .Include(i => i.Fields.OrderBy(f => f.Order))
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.ApiToken == token);

            if (inventory == null)
                return Unauthorized(new { error = "Invalid API token." });

            var stats = _statService.ComputeStats(inventory.Fields, inventory.Items);

            var response = new InventoryApiResponse
            {
                InventoryId = inventory.Id,
                Title       = inventory.Title,
                Description = inventory.Description,
                ItemCount   = inventory.Items.Count,
                Fields      = stats.Select(s => new FieldApiDto
                {
                    Name         = s.FieldName,
                    Type         = s.Type.ToString(),
                    Slot         = s.Slot,
                    TotalItems   = s.TotalItems,
                    FilledCount  = s.FilledCount,
                    FillPercent  = s.FillPercent,
                    NumMin       = s.NumMin,
                    NumMax       = s.NumMax,
                    NumAvg       = s.NumAvg,
                    TopValues    = s.TopValues
                                    .Select(tv => new TopValueDto { Value = tv.Value, Count = tv.Count })
                                    .ToList(),
                    BoolTrueCount  = s.BoolTrueCount,
                    BoolFalseCount = s.BoolFalseCount,
                    BoolNullCount  = s.BoolNullCount
                }).ToList()
            };

            return Ok(response);
        }

        // ── DTO types ──────────────────────────────────────────────────────────

        public class InventoryApiResponse
        {
            public int    InventoryId { get; set; }
            public string Title       { get; set; } = "";
            public string Description { get; set; } = "";
            public int    ItemCount   { get; set; }
            public List<FieldApiDto> Fields { get; set; } = new();
        }

        public class FieldApiDto
        {
            public string Type        { get; set; } = "";
            public string Name        { get; set; } = "";
            public string Slot        { get; set; } = "";
            public int    TotalItems  { get; set; }
            public int    FilledCount { get; set; }
            public int    FillPercent { get; set; }

            // Number stats
            public int?    NumMin { get; set; }
            public int?    NumMax { get; set; }
            public double? NumAvg { get; set; }

            // String / Text stats
            public List<TopValueDto> TopValues { get; set; } = new();

            // Bool stats
            public int BoolTrueCount  { get; set; }
            public int BoolFalseCount { get; set; }
            public int BoolNullCount  { get; set; }
        }

        public class TopValueDto
        {
            public string Value { get; set; } = "";
            public int    Count { get; set; }
        }
    }
}
