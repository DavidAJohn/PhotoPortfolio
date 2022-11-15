namespace PhotoPortfolio.Shared.Models;

public class UploadResult
{
    public bool Uploaded { get; set; }
    public string? FileName { get; set; }
    public string? StoredFileName { get; set; }
    public string? AzureUri { get; set; }
    public string? Title { get; set; }
    public string? Subject { get; set; }
    public int ErrorCode { get; set; }
    public PhotoMetadata? Metadata { get; set; }
}
