using Inventory_Managment.Models;
using Inventory_Managment.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Managment.Filters
{
    public class ItemEditFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ItemEditFilter(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            int inventoryId;

            if (context.ActionArguments.ContainsKey("inventoryId"))
            {
                inventoryId = (int)context.ActionArguments["inventoryId"];
            }
            else if (context.ActionArguments.ContainsKey("item"))
            {
                var item = context.ActionArguments["item"] as Item;

                if (item == null)
                {
                    context.Result = new BadRequestResult();
                    return;
                }
                inventoryId = item.InventoryId;
            }
            else
            {
                context.Result = new BadRequestResult();
                return;
            }

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);

            if (inventory == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            var user = context.HttpContext.User;
            var userId = _userManager.GetUserId(user);
            var isAdmin = user.IsInRole("Admin");

            var hasAccess = await _context.InventoryAccess
                .AnyAsync(a => a.InventoryId == inventoryId && a.UserId == userId);

            if (
                !isAdmin &&
                inventory.CreatorId != userId &&
                !inventory.IsPublic &&
                !hasAccess
            )
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}
