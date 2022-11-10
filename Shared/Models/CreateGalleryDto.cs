using PhotoPortfolio.Shared.Entities;

namespace PhotoPortfolio.Shared.Models;

public class CreateGalleryDto : BaseEntity
{
    public string Name { get; set; } = "New Gallery";
    public string Description { get; set; } = string.Empty;
    public string HeaderImage { get; set; } = "/images/default_header_image.png";
    public string HeaderImageAlign { get; set; } = "center";
    public string? ParentGallery { get; set; }
    public string SortBy { get; set; } = string.Empty;
    public string SortOrder { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public List<Photo>? Photos { get; set; } = new();
    public bool Public { get; set; } = false;
    public bool DisplayInGalleryList { get; set; } = false;
}
