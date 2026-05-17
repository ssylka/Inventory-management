using Inventory_Managment.Models.Directory;

namespace Inventory_Managment.Models.Dto
{
    public class FieldStatDto
    {
        public string    FieldName   { get; set; } = "";
        public string    Slot        { get; set; } = "";
        public FieldType Type        { get; set; }
        public int       TotalItems  { get; set; }
        public int       FilledCount { get; set; }

        public int FillPercent => TotalItems == 0 ? 0
            : (int)Math.Round(FilledCount * 100.0 / TotalItems);

        public int?    NumMin { get; set; }
        public int?    NumMax { get; set; }
        public double? NumAvg { get; set; }   

        public List<(string Value, int Count)> TopValues { get; set; } = new();

        public int BoolTrueCount  { get; set; }
        public int BoolFalseCount { get; set; }
        public int BoolNullCount  { get; set; }

    }
}
