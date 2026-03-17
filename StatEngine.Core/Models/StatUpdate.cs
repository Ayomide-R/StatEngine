namespace StatEngine.Core.Models;

public class StatUpdate
{
    public string Id { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string RawContent { get; set; } = string.Empty;
    public string RefinedContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ImagePath { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
