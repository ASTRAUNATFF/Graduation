using Kindergarten_school.DTO;
using Kindergarten_school.Extensions;
using Kindergarten_school.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;


namespace Kindergarten_school.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IAttendanceService _attendanceService;
        private readonly IMongoCollection<StudentModel> _students;
        private readonly IMongoCollection<Attendance> _attendance;

        public AttendanceController(IMongoDatabase database, IStudentService studentService, IAttendanceService attendanceService)
        {
            _students = database.GetCollection<StudentModel>("students");
            _attendance = database.GetCollection<Attendance>("attendances");
            _studentService = studentService;
            _attendanceService = attendanceService;
        }

        public async Task<IActionResult> Index(int selectedMonth = 5)
        {
            ViewBag.SelectedMonth = selectedMonth;

            var daysInMonth = DateTime.DaysInMonth(DateTime.Today.Year, selectedMonth);
            ViewBag.DaysInMonth = daysInMonth;

            // Load attendance data theo tháng
            var startDate = new DateTime(DateTime.Today.Year, selectedMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var attendanceList = await _attendanceService.GetAttendanceByDateRangeAsync(startDate, endDate);
            var students = await _studentService.GetAllAsync();

            // Kiểm tra danh sách học sinh
            if (students == null || !students.Any())
            {
                Console.WriteLine("Không có học sinh nào trong danh sách.");
                return View(new AttendanceViewModel { Students = new List<StudentModel>(), AttendanceRecords = new List<Attendance>(), SelectedMonth = selectedMonth });
            }

            // Truyền dữ liệu vào model
            var viewModel = new AttendanceViewModel
            {
                Students = students,
                AttendanceRecords = attendanceList,
                SelectedMonth = selectedMonth,
            };

            return View(viewModel);
        }


        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult SubmitAttendance(List<Attendance> attendanceRecords)
        {
            if (attendanceRecords == null || !attendanceRecords.Any())
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            // Kiểm tra quyền
            var role = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "admin" && role != "teacher")
            {
                return Forbid();
            }

            var date = DateTime.Today;

            // Lưu dữ liệu điểm danh: chỉ lưu khi có P hoặc K
            var filteredRecords = attendanceRecords
                .Where(r => r.Status == "P" || r.Status == "K" || r.Status == "Vắng")
                .Select(r => new Attendance
                {
                    StudentID = r.StudentID,
                    FullName = r.FullName,
                    Date = date,
                    Status = r.Status
                })
                .ToList();

            if (filteredRecords.Any())
            {
                _attendance.InsertMany(filteredRecords);
            }

            return RedirectToAction("Index");
        }

        [HttpPost("attendance/check-box")]
        public async Task<IActionResult> PostAttendance([FromForm] AttendanceDto dto)
        {
            try
            {
                // Lấy AttendanceID cao nhất hiện tại
                var highestAttendanceRecord = await _attendance
                    .Find(FilterDefinition<Attendance>.Empty)
                    .SortByDescending(a => a.AttendanceID)
                    .FirstOrDefaultAsync();

                int newAttendanceID = highestAttendanceRecord == null ? 1 : highestAttendanceRecord.AttendanceID + 1;

                // Kiểm tra nếu đã có bản ghi cho ngày này
                var filter = Builders<Attendance>.Filter.Eq(a => a.StudentID, dto.StudentId) &
                             Builders<Attendance>.Filter.Eq(a => a.Date, dto.Date.Date);

                var existingRecord = await _attendance.Find(filter).FirstOrDefaultAsync();

                if (existingRecord != null)
                {
                    // Nếu đã có thì cập nhật
                    // Kiểm tra nếu học phí đã được tính
                    if (existingRecord.IsTuitionCalculated)
                    {
                        // Nếu học phí đã được tính, không cho phép cập nhật
                        return BadRequest(new { status = false, message = "Không thể cập nhật vì học phí đã được tính" });
                    }
                    var update = Builders<Attendance>.Update.Set(a => a.Status, dto.Status);
                    await _attendance.UpdateOneAsync(filter, update);
                }
                else
                {
                    // Nếu chưa có thì thêm mới với AttendanceID mới
                    var newAttendance = new Attendance
                    {
                        AttendanceID = newAttendanceID,
                        StudentID = dto.StudentId,
                        Date = dto.Date,
                        Status = dto.Status,
                        IsChecked = dto.IsChecked,
                        IsTuitionCalculated = false

                    };
                    await _attendance.InsertOneAsync(newAttendance);
                }

                return Ok(new { status = true, message = "Đã lưu điểm danh" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = "Có lỗi xảy ra", error = ex.Message });
            }
        }



    }
}