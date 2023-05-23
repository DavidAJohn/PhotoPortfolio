namespace PhotoPortfolio.Shared.Models;

public class BreadcrumbCrumb
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Uri { get; set; }
    public bool Enabled { get; set; }
}
