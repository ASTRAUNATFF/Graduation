using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Kindergarten_school.Models
{
    public class Attendance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public DateTime Date { get; set; }

        public string? Status { get; set; } // "Có Mặt" hoặc "Vắng"

        public int StudentID { get; set; }
        
        [BsonElement("AttendanceID")] // Ánh xạ với trường `AttendanceID` trong cơ sở dữ liệu
        public int AttendanceID { get; set; }
    }
}
