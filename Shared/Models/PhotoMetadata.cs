namespace PhotoPortfolio.Shared.Models;

public class PhotoMetadata
{
    public List<string>? Tags { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? CameraMake { get; set; } = "Unknown";
    public string? CameraModel { get; set; } = "Unknown";
    public string? LensMake { get; set; } = "Unknown";
    public string? LensModel { get; set; } = "Unknown";
    public string? ShutterSpeed { get; set; } = "Unknown";
    public string? Aperture { get; set; } = "Unknown";
    public string? Iso { get; set; } = "Unknown";
    public string? FocalLength { get; set; } = "Unknown";
    public string? DateTaken { get; set; } = "Unknown";
}
