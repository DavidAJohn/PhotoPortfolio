namespace PhotoPortfolio.Shared.Models;

public class UploadResult
{
    public bool Uploaded { get; set; }
    public string? FileName { get; set; }
    public string? StoredFileName { get; set; }
    public string? AzureUri { get; set; }
    public string? Title { get; set; }
    public string? Subject { get; set; }
    public UploadErrorCode ErrorCode { get; set; }
    public List<string>? ErrorMessages { get; set; }
    public PhotoMetadata? Metadata { get; set; }
}

public enum UploadErrorCode
{ 
    Success = 0,
    ImageProcessingError = 1,
    BasicFileCheckError = 2,
    AzureUploadError = 3,
    UnexpectedError = 4
}
