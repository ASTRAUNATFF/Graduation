using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class TeacherModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("FirstName")]
        public string? FirstName { get; set; }

        [BsonElement("LastName")]
        public string? LastName { get; set; }

        [BsonElement("Subject")]
        public string? Subject { get; set; }

        [BsonElement("Phone")]
        public string? Phone { get; set; }

        [BsonElement("HireDate")]
        public string? HireDate { get; set; }

        [BsonElement("Adress")]
        public string? Address { get; set; }

        [BsonElement("TeacherID")]
        public int TeacherID { get; set; }
    }
}
