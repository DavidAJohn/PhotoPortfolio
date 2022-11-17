using MongoDB.Bson.Serialization.Attributes;

namespace PhotoPortfolio.Shared.Models;

[BsonIgnoreExtraElements]
public class PhotoMetadata
{
    public List<string>? Tags { get; set; }
    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;
    public string? CameraMake { get; set; } = "";
    public string? CameraModel { get; set; } = "";
    public string? LensMake { get; set; } = "";
    public string? LensModel { get; set; } = "";
    public string? ShutterSpeed { get; set; } = "";
    public string? Aperture { get; set; } = "";
    public string? Iso { get; set; } = "";
    public string? FocalLength { get; set; } = "";
    public string? DateTaken { get; set; } = "";
}
