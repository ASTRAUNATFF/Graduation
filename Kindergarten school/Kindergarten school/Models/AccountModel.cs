using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace Kindergarten_school.Models
{
    public class AccountModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("AccountID")]
        public string? AccountID { get; set; } // Liên kết với ParentID, TeacherID, v.v.

        [BsonElement("Username")]
        public string? Username { get; set; }

        [BsonElement("PasswordHash")]
        public string? PasswordHash { get; set; }

        [BsonElement("Role")]
        public string? Role { get; set; } // e.g., "parent", "teacher", "admin"

        [BsonElement("Phone")]
        public string? Phone { get; set; }

        [BsonElement("Email")]
        public string? Email { get; set; }
        
        [BsonElement("IsActive")]
        public string? IsActive { get; set; }

        [BsonElement("CreatedAt")]
        public string? CreatedAt { get; set; }

        [BsonElement("UpdateAt")]
        public string? UpdateAt { get; set; }
    }
}
