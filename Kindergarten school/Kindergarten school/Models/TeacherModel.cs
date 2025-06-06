﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class TeacherModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("AccountID")]
        public string? AccountID { get; set; }

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
        [BsonElement("Email")]
        public string? Email { get; set; }

        [BsonElement("Adress")]
        public string? Address { get; set; }

        [BsonElement("TeacherID")]
        public string? TeacherID { get; set; }

        [BsonElement("ClassID")]
        public string? ClassID { get; set; }
        
        [BsonElement("PasswordHash")]
        public string? PasswordHash { get; set; }
        [BsonElement("Roles")]
        public string? Roles { get; set; }
        [BsonElement("PhotoURL")]
        public string? PhotoURL { get; set; }
        [BsonElement("IsActive")]
        public string? IsActive { get; set; }

    }
}
