namespace Kindergarten_school.Models
{
    public class ScheduleCreateModel
    {
        public string? TeacherSelection { get; set; } // ví dụ: "GV001|CL001"

        public string? TeacherID { get; set; }
        public string? ClassID { get; set; }

        public string? LastName { get; set; }

        // Các property khác tùy form create
        public int TimeSlot { get; set; }
        public string? Activity { get; set; }

        public int DayOfWeek { get; set; }



    }
}
