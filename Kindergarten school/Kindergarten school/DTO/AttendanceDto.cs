namespace Kindergarten_school.DTO
{
    public class AttendanceDto
    {
        public string StudentId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Vắng";
        public bool IsChecked { get; set; } = false;
    }
}
