namespace PhotoPortfolio.Shared.Helpers;

public class OrderSpecificationParams
{
    public string? Status { get; set; }
    public string? CustomerEmail { get; set; }
    public int InLastNumberOfDays { get; set; } = 365;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public bool ExcludePaymentIncomplete { get; set; } = false;
}
