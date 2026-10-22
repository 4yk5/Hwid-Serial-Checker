using System.IO;
using System.Text.Json;

namespace HwidSerialChecker;

public class ComparisonRow
{
    public string Category { get; set; } = string.Empty;
    public string Property { get; set; } = string.Empty;
    public string Tag { get; set; } = "Spoofable";
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public bool IsChanged { get; set; }
    public string Details { get; set; } = string.Empty;
}

public static class ComparisonEngine
{
    private static readonly string StoragePath = Path.Combine(AppContext.BaseDirectory, "baseline_serials.json");

    public static bool HasBaseline()
    {
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string reportFile = Path.Combine(desktop, "OG_OriginalSerials.html");
        return File.Exists(StoragePath) && File.Exists(reportFile);
    }

    public static void SaveBaseline(List<HardwareItem> items)
    {
        try
        {
            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(StoragePath, json);
        }
        catch { }
    }

    public static List<HardwareItem>? LoadBaseline()
    {
        try
        {
            if (!File.Exists(StoragePath)) return null;
            var json = File.ReadAllText(StoragePath);
            return JsonSerializer.Deserialize<List<HardwareItem>>(json);
        }
        catch
        {
            return null;
        }
    }

    public static List<ComparisonRow> Compare(List<HardwareItem> oldItems, List<HardwareItem> newItems)
    {
        var result = new List<ComparisonRow>();

        var oldMap = oldItems.ToDictionary(
            i => $"{i.Category}||{i.Property}",
            i => i,
            StringComparer.OrdinalIgnoreCase
        );

        var newMap = newItems.ToDictionary(
            i => $"{i.Category}||{i.Property}",
            i => i,
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var newItem in newItems)
        {
            string key = $"{newItem.Category}||{newItem.Property}";
            if (oldMap.TryGetValue(key, out var oldItem))
            {
                bool changed = !string.Equals(oldItem.Value.Trim(), newItem.Value.Trim(), StringComparison.OrdinalIgnoreCase);
                result.Add(new ComparisonRow
                {
                    Category = newItem.Category,
                    Property = newItem.Property,
                    Tag = newItem.Tag,
                    OldValue = oldItem.Value,
                    NewValue = newItem.Value,
                    IsChanged = changed,
                    Details = newItem.Details
                });
            }
            else
            {
                result.Add(new ComparisonRow
                {
                    Category = newItem.Category,
                    Property = newItem.Property,
                    Tag = newItem.Tag,
                    OldValue = "(Not in previous scan)",
                    NewValue = newItem.Value,
                    IsChanged = true,
                    Details = newItem.Details
                });
            }
        }

        foreach (var oldItem in oldItems)
        {
            string key = $"{oldItem.Category}||{oldItem.Property}";
            if (!newMap.ContainsKey(key))
            {
                result.Add(new ComparisonRow
                {
                    Category = oldItem.Category,
                    Property = oldItem.Property,
                    Tag = oldItem.Tag,
                    OldValue = oldItem.Value,
                    NewValue = "(Not in new scan)",
                    IsChanged = true,
                    Details = oldItem.Details
                });
            }
        }

        return result;
    }
}
