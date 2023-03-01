using MongoDB.Bson;

namespace PhotoPortfolio.Shared.Entities;

public class Preferences : BaseEntity
{
    public string SiteName { get; set; } = "Photo Portfolio";
    public Dictionary<string, string> Metadata { get; set; }
}
