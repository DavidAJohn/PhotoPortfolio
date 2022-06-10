namespace PhotoPortfolio.Shared.Entities;

public class Gallery : BaseEntity
{
    public string Name { get; set; } = "New Gallery";
    public string Description { get; set; } = "Gallery Description";
    public string HeaderImage { get; set; } = null!;
    public string? ParentGallery { get; set; }
    public string SortBy { get; set; } = string.Empty;
    public string SortOrder { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
