namespace Kindergarten_school.Models
{
    public class BehaviorCreateViewModel
    {
        public string SelectedStudentId { get; set; }
        public string SelectedClassId { get; set; }
        public string Title { get; set; }
        public string Decription { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;

        public List<StudentModel>? Students { get; set; }
        public List<ClassModel>? Classes { get; set; }
    }
}
