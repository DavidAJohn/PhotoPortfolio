using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PhotoPortfolio.Shared.Entities;

public abstract class BaseEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
}
