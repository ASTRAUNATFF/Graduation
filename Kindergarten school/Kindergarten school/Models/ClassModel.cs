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
        public string? ClassID { get; set; }

        [Required]
        [Display(Name = "ID giáo viên phụ trách")]

        public string? TeacherID { get; set; }// Thêm TeacherID để xác định giáo viên phụ trách



        // Danh sách học sinh trong lớp
        public List<StudentModel> Students { get; set; } = new List<StudentModel>();
    }

}
