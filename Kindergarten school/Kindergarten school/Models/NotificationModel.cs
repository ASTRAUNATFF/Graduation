using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Kindergarten_school.Models
{
    public class NotificationModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Title")]
        public string? Title { get; set; } // Tiêu đề thông báo

        [BsonElement("Message")]
        public string? Message { get; set; } // Nội dung thông báo

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } // Ngày tạo thông báo
    }
}
