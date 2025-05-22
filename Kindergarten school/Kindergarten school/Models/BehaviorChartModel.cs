using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class BehaviorChartModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("StudenId")]
        public int StudenId { get; set; }

        [BsonElement("ClassId")]
        public int ClassId { get; set; }

        [BsonElement("Title")]
        public string Title { get; set; }


        [BsonElement("Decription")]
        public string Decription { get; set; }

        [BsonElement("CreateDate")]
        public DateTime CreateDate { get; set; }
    }
}
