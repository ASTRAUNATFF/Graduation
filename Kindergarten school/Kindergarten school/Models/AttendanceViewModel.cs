namespace Kindergarten_school.Models
{
    public class AttendanceViewModel
    {
        public List<StudentModel> Students { get; set; }
        public List<Attendance> AttendanceRecords { get; set; }
        public int SelectedMonth { get; set; } = DateTime.Now.Month;
        public string? Status { get; set; }
        public string? FullName { get; set; }
        public string? StudentID { get; set; }
        public bool IsChecked { get; set; }
        public bool IsTuitionCalculated { get; set; }
    }
}
