using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class NotificationSchool
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("type")]
        public string? type { get; set; }

        [BsonElement("studentId")]
        public int studentId { get; set; }

        [BsonElement("classID")]
        public int classID { get; set; }

        [BsonElement("createDate")]
        public DateTime createDate { get; set; }

        [BsonElement("content")]
        public string? content { get; set; }
    }
}