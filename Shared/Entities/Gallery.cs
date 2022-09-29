namespace PhotoPortfolio.Shared.Entities;

public class Gallery : BaseEntity
{
    public string Name { get; set; } = "New Gallery";
    public string Description { get; set; } = string.Empty;
    public string HeaderImage { get; set; } = null!;
    public string HeaderImageAlign { get; set; } = "center";
    public string? ParentGallery { get; set; }
    public string SortBy { get; set; } = string.Empty;
    public string SortOrder { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public List<Photo> Photos { get; set; } = null!;
    public bool Public { get; set; } = false;
}
