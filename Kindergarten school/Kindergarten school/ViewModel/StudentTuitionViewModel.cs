using Kindergarten_school.Models;

namespace Kindergarten_school.ViewModel
{
    public class StudentTuitionViewModel
    {
        public StudentModel Student { get; set; }
        public List<TuitionFeesModel> TuitionFees { get; set; }
        public List<Attendance> AttendedRecords { get; set; }  // Danh sách điểm danh đã có mặt
        public List<Attendance> MissedRecords { get; set; }    // Danh sách điểm danh vắng mặt
    }
}
