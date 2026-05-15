using Inventory_Managment.Models;
using Inventory_Managment.Models.Directory;
using Inventory_Managment.Models.Dto;

namespace Inventory_Managment.Services
{
    public class StatService
    {
        public List<FieldStatDto> ComputeStats(
            IEnumerable<InventoryField> fields,
            IList<Item> items)
        {
            var stats = new List<FieldStatDto>();

            foreach (var field in fields)
            {
                if (string.IsNullOrEmpty(field.Slot))
                    continue;

                var property = typeof(Item).GetProperty(field.Slot);

                if (property == null)
                    continue;

                var values = items
                    .Select(i => property.GetValue(i))
                    .ToList();

                var stat = CreateBaseStat(field, items.Count);

                FillStatistics(stat, field.Type, values);

                stats.Add(stat);
            }

            return stats;
        }

        private FieldStatDto CreateBaseStat(
            InventoryField field,
            int totalItems)
        {
            return new FieldStatDto
            {
                FieldName = field.Name,
                Slot = field.Slot!,
                Type = field.Type,
                TotalItems = totalItems
            };
        }

        private void FillStatistics(
            FieldStatDto stat,
            FieldType type,
            List<object?> values)
        {
            switch (type)
            {
                case FieldType.Number:
                    FillNumberStats(stat, values);
                    break;

                case FieldType.String:
                case FieldType.Text:
                    FillStringStats(stat, values);
                    break;

                case FieldType.Link:
                    FillLinkStats(stat, values);
                    break;

                case FieldType.Bool:
                    FillBoolStats(stat, values);
                    break;
            }
        }

        private void FillNumberStats(
            FieldStatDto stat,
            List<object?> values)
        {
            var numbers = values
                .OfType<int>()
                .ToList();

            stat.FilledCount = numbers.Count;

            if (numbers.Count == 0)
                return;

            stat.NumMin = numbers.Min();
            stat.NumMax = numbers.Max();
            stat.NumAvg = Math.Round(numbers.Average(), 1);
        }

        private void FillStringStats(
            FieldStatDto stat,
            List<object?> values)
        {
            var strings = values
                .OfType<string>()
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            stat.FilledCount = strings.Count;

            stat.TopValues = strings
                .GroupBy(s => s)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => (g.Key, g.Count()))
                .ToList();
        }

        private void FillLinkStats(
            FieldStatDto stat,
            List<object?> values)
        {
            stat.FilledCount = values
                .OfType<string>()
                .Count(s => !string.IsNullOrWhiteSpace(s));
        }

        private void FillBoolStats(
            FieldStatDto stat,
            List<object?> values)
        {
            foreach (var value in values)
            {
                if (value is bool b)
                {
                    if (b)
                        stat.BoolTrueCount++;
                    else
                        stat.BoolFalseCount++;
                }
                else
                {
                    stat.BoolNullCount++;
                }
            }

            stat.FilledCount =
                stat.BoolTrueCount +
                stat.BoolFalseCount;
        }
    }
}