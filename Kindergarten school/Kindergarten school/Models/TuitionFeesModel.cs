using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kindergarten_school.Models
{
    public class TuitionFeesModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public DateTime Date { get; set; }


        public string? Status { get; set; } // "Đã thanh toán / Đang chờ"

        [BsonRepresentation(BsonType.String)]
        public string? StudentID { get; set; }

        public decimal Amount { get; set; } // Số tiền thanh toán

        public string Title { get; set; } // Tiêu đề thanh toán
        public int TotalDaysAttended { get; set; }

    }
}
