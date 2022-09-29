namespace PhotoPortfolio.Shared.Models;

public class TextBlockItem
{
    public string Id { get; set; } = string.Empty;
    public string TextBlockId { get; set; } = string.Empty;
    public int BlockSortOrder { get; set; }
    public string TextContent { get; set; } = string.Empty;
    public string TextColour { get; set; } = "#000000";
    public string TextSize { get; set; } = "md";
    public string TextWeight { get; set; } = "normal";
    public string TextAlignment { get; set; } = "left";
}
