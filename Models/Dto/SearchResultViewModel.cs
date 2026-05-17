namespace Inventory_Managment.Models.Dto
{
    public class SearchResultViewModel
    {
        public string Query { get; set; } = "";
        public List<Inventory> Inventories { get; set; } = new();
        public List<Item> Items { get; set; } = new();
    }
}
