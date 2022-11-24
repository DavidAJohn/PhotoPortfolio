using MongoDB.Bson.Serialization.Attributes;

namespace PhotoPortfolio.Shared.Entities;

[BsonIgnoreExtraElements]
public class Product : BaseEntity
{
    public string ProdigiSku { get; set; } = string.Empty;
    public string ProdigiDescription { get; set; } = string.Empty;
    public List<string> ProdigiImageAssetUris { get; set; } = null!;
}
