namespace PhotoPortfolio.Shared.Entities;

public class Photo : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Uri { get; set; }
    public string? GalleryId { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    public bool ShowAsFavourite { get; set; } = false;
}
