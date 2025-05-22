using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class HealthReport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("studentId")]
        public int studentId { get; set; }

        [BsonElement("createDate")]
        public DateTime createDate { get; set; }

        [BsonElement("HealthStatus")]
        public string? HealthStatus { get; set; }
    }
}