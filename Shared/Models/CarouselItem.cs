namespace PhotoPortfolio.Shared.Models;

public enum ImageAlignment
{
    Top,    // 0
    Middle, // 1
    Bottom, // 2
}

public class CarouselItem
{
    public string Uri { get; set; } = string.Empty;
    public ImageAlignment ImageAlign { get; set; } = ImageAlignment.Middle;
}
