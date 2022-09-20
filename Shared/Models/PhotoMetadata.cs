namespace PhotoPortfolio.Shared.Models;

public class PhotoMetadata
{
    public List<string>? Tags { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? ShutterSpeed { get; set; }
    public string? Aperture { get; set; }
    public string? Iso { get; set; }
    public string? FocalLength { get; set; }
    public string? DateTaken { get; set; }
}
