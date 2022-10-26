namespace PhotoPortfolio.Shared.Entities;

public class Product : BaseEntity
{
    public string PhotoId { get; set; } = string.Empty;
    public string ProdigiSku { get; set; } = string.Empty;
    public string ProdigiDescription { get; set; } = string.Empty;
    public string CustomDescription { get; set; } = string.Empty;
    public string FurtherDetails { get; set; } = string.Empty;
    public string MockupImageUri { get; set; } = string.Empty;
    public List<string> ProdigiImageAssetUris { get; set; } = null!;
    public int MarkupPercentage { get; set; } = 100;
}
