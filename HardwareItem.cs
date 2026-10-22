namespace HwidSerialChecker;

public class HardwareItem
{
    public string Category { get; set; } = string.Empty;
    public string Property { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Tag { get; set; } = "Spoofable";
    public string Details { get; set; } = string.Empty;

    public HardwareItem() { }

    public HardwareItem(string category, string property, string? value, string tag = "Spoofable", string details = "")
    {
        Category = category;
        Property = property;
        Value = string.IsNullOrWhiteSpace(value) ? "(N/A)" : value.Trim();
        Tag = tag;
        Details = details;
    }
}
