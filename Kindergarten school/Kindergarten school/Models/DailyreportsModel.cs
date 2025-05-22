using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class DailyreportsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("ReportDate")]
        public DateTime ReportDate { get; set; }

        [BsonElement("Activities")]
        public string? Activities { get; set; }

        [BsonElement("Meal")]
        public string? Meal { get; set; }

        [BsonElement("NapTime")]
        public string? NapTime { get; set; }

        [BsonElement("ReportID")]
        public int ReportID { get; set; }


        [BsonElement("ClassID")]
        public int ClassID { get; set; }

        [BsonElement("Description")]
        public string? Description { get; set; }

        [BsonElement("StudentID")]
        public int StudentID { get; set; }

    }
}