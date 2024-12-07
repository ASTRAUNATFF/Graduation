using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class Parent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PasswordHash { get; set; }
        public int ParentID { get; set; }
    }
}
