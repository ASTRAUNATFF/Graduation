using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Kindergarten_school.Models
{
    public class Fee
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("FeeID")] // Đảm bảo ánh xạ đúng với MongoDB
        public int FeeID { get; set; }

        [BsonElement("StudentID")] // Đảm bảo ánh xạ đúng với MongoDB
        public int StudentId { get; set; }

        [BsonElement("FeeTypes")] // Thêm FeeTypes để ánh xạ với MongoDB
        public string? FeeTypes { get; set; } // Loại phí (ví dụ: "Phí Đầu Năm", "Phí Bảo Hiểm")

        [BsonElement("Amount")]
        public decimal Amount { get; set; } // Số tiền học phí

        [BsonElement("DaysAttended")]
        public int DaysAttended { get; set; } // Số ngày đi học

        [BsonElement("Status")]
        public string? Status { get; set; } // "Đã Đóng" hoặc "Chưa Đóng"

        [BsonElement("DueDate")] // Thêm DueDate để ánh xạ đúng với MongoDB
        public DateTime DueDate { get; set; } // Ngày đến hạn đóng học phí

        [BsonElement("LastUpdated")]
        public DateTime LastUpdated { get; set; }

        [BsonElement("AttendanceID")] // Liên kết với Attendance
        public int AttendanceId { get; set; }
    }
}
