using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Kindergarten_school.Models
{
    public class ClassModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [Display(Name = "Tên lớp")]
        public string? ClassName { get; set; }

        [Required]
        [Display(Name = "Số lượng học sinh tối đa")]
        public int MaxStudent { get; set; }

        [Display(Name = "Thời khóa biểu")]
        public DateTime Schedule { get; set; }

        [Display(Name = "ID lớp học")]
        public int ClassID { get; set; }
    }
}
