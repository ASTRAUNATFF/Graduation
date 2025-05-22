using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class FeedbackModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ParentID")]
        public string? ParentID { get; set; } // Giữ lại ParentID

        [BsonElement("Phone")]
        public string? Phone { get; set; } // Thêm Phone vào model

        [BsonElement("Content")]
        public string? Content { get; set; }

        [BsonElement("CreateAt")]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    }
}
