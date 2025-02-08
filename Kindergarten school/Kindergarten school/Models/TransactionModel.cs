using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Kindergarten_school.Models
{
    public class TransactionModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? transactionID { get; set; }
        public int studentID { get; set; }
        public decimal amount { get; set; }
        public string? typeoftrans { get; set; }
        public DateTime transactiondate { get; set; }
    }
}
