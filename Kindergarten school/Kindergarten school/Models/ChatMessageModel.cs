using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class ChatMessageModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ConversationId")]
        public string ConversationId { get; set; } // ID của cuộc hội thoại

        [BsonElement("SenderId")]
        public string SenderId { get; set; } // ID của người gửi (Parent hoặc Teacher)

        [BsonElement("Message")]
        public string Message { get; set; }

        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
