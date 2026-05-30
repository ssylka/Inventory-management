namespace Inventory_Managment.Models.Dto
{
    public class SupportTicketViewModel
    {
        public string Summary { get; set; } = "";
        public string Priority { get; set; } = "Average";
        public string ReportedBy { get; set; } = "";
        public string? InventoryTitle { get; set; }
        public string PageLink { get; set; } = "";
        public string AdminEmails { get; set; } = "";
    }
}