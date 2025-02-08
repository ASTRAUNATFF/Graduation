using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Kindergarten_school.Models
{
    public class ScheduleModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ClassId")]
        public string? ClassId { get; set; }

        [BsonElement("DayOfWeek")]
        public string? DayOfWeek { get; set; } // Monday, Tuesday, etc.

        [BsonElement("StartTime")]
        public DateTime StartTime { get; set; }

        [BsonElement("EndTime")]
        public DateTime EndTime { get; set; }

        [BsonElement("Subject")]
        public string? Subject { get; set; }

        [BsonElement("TeacherId")]
        public string? TeacherId { get; set; }
    }
}
