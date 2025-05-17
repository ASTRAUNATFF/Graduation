using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class Parent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? AccountID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PasswordHash { get; set; }
        public string? ParentID { get; set; }
        public string? Roles { get; set; }
        public string? TeacherID { get; set; }
        public string? ClassID { get; set; }

        public string? Address { get; set; }

        public string? StudentID { get; set; }
        public string? Gender { get; set; }
        public DateTime DateofBirth { get; set; }
        public string? PhotoURL { get; set; }
    }
}
