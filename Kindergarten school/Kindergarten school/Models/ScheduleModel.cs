using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class ScheduleModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ClassId")]
        public string? ClassId { get; set; }

        [BsonElement("ClassName")]
        public string? ClassName { get; set; }

        [BsonElement("DayOfWeek")]
        public int DayOfWeek { get; set; }

        [BsonElement("TimeSlot")]
        public int TimeSlot { get; set; }

        [BsonElement("Activity")]
        public string? Activity { get; set; }

        [BsonElement("Fixed")]
        public bool Fixed { get; set; }

        [BsonElement("TeacherName")]
        public string? TeacherName { get; set; }

        [BsonElement("TeacherId")]
        public string? TeacherId { get; set; }

    }
}