using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace WebApplication1.Models
{
    public interface IEntity
    { 
        List<Address>? Addresses { get; }
        List<Date> Dates { get; }
        bool Deceased { get; }
        string? Gender { get;}
        List<Name>? Names { get; }
    };

    public class Entity : IEntity
    {
        [BsonId]

        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public List<Address>? Addresses { get; set; }
        public List<Date> Dates { get; set; }
        public bool Deceased { get; set; }
        public string? Gender { get; set; }
        public List<Name>? Names { get; set; }
        
        
    };
}