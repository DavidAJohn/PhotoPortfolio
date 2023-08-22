using PhotoPortfolio.Shared.Entities;
using System.ComponentModel.DataAnnotations;

namespace PhotoPortfolio.Shared.Models;

public class CreatePhotoProductDto : Product
{
    [Required(ErrorMessage = "Custom Description can not be empty")]
    public string? CustomDescription { get; set; }

    [Required(ErrorMessage = "Further Details can not be empty")]
    public string? FurtherDetails { get; set; }

    public string? MockupImageUri { get; set; }

    [Required(ErrorMessage = "Markup Percentage can not be empty")]
    [Range(0, 1000, ErrorMessage = "Value must be between {1} and {2}.")]
    public int MarkupPercentage { get; set; } = 100;
}
