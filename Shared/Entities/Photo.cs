using PhotoPortfolio.Shared.Models;

namespace PhotoPortfolio.Shared.Entities;

public class Photo : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Uri { get; set; }
    public string? GalleryId { get; set; }
    public int GallerySortOrder { get; set; } = 0;
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    public PhotoMetadata? Metadata { get; set; }
}
