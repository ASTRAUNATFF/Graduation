using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class Attendance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public DateTime Date { get; set; }

        public string? Status { get; set; } // "Có Mặt" hoặc "Vắng"

        [BsonRepresentation(BsonType.String)]
        public string? StudentID { get; set; }

        [BsonElement("AttendanceID")] // Ánh xạ với trường `AttendanceID` trong cơ sở dữ liệu
        public int AttendanceID { get; set; }

        public string? FullName { get; set; }

        public bool IsChecked { get; set; }
        public bool IsTuitionCalculated { get; set; }


    }
}
