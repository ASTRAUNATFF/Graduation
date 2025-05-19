using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class ChatConversationModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Participants")]
        public List<string> Participants { get; set; } = new List<string>();
    }
}
