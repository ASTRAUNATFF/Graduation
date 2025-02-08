using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Kindergarten_school.Models
{
    public class StudentModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("StudentID")] // Ánh xạ trường StudentID từ MongoDB
        public string? StudentID { get; set; }

        [BsonElement("FirstName")]
        public string? FirstName { get; set; }

        [BsonElement("LastName")]
        public string? LastName { get; set; }

        [BsonElement("DateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [BsonElement("Age")]
        public int Age
        {
            get
            {
                return DateTime.Now.Year - DateOfBirth.Year -
                       (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
            }
        }

        [BsonElement("Gender")]
        public string? Gender { get; set; }

        [BsonElement("Address")]
        public string? Address { get; set; }

        [BsonElement("ParentContact")]
        public string? ParentContact { get; set; }

        [BsonElement("ClassId")]
        public string? ClassId { get; set; }

        [BsonElement("EnrollmentDate")]
        public DateTime EnrollmentDate { get; set; }

        [BsonElement("HealthStatus")]
        public string? HealthStatus { get; set; }

        [BsonElement("ParentID")]
        public string? ParentID { get; set; }
        
        [BsonElement("TeacherID")]
        public string? TeacherID { get; set; }
    }
}
