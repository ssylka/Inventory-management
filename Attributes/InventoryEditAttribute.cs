using Inventory_Managment.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Managment.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InventoryEditAttribute : TypeFilterAttribute
    {
        public InventoryEditAttribute() : base(typeof(InventoryEditFilter)) { }
    }
}
