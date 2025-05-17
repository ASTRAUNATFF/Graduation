using Kindergarten_school.Models;
using Kindergarten_school.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Kindergarten_school.Controllers
{
    [Authorize(Roles = "admin")]

    public class TuitionFeesController : Controller
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<ClassModel> _classCollection;
        private readonly IMongoCollection<StudentModel> _studentCollection;
        private readonly IMongoCollection<TeacherModel> _teacherCollection;
        private readonly IMongoCollection<TuitionFeesModel> _tuitionFeesModel;
        private readonly IMongoCollection<Attendance> _attendance;
        private readonly IMongoDatabase _database;
        public TuitionFeesController(IMongoDatabase database, IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase("db2024");
            _studentCollection = database.GetCollection<StudentModel>("students");
            _classCollection = database.GetCollection<ClassModel>("classes");
            _teacherCollection = database.GetCollection<TeacherModel>("teachers");
            _tuitionFeesModel = database.GetCollection<TuitionFeesModel>("TuitionFees");
            _attendance = database.GetCollection<Attendance>("attendances");

        }


        [HttpGet("admin/tuition-fees/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var studentFilter = Builders<StudentModel>.Filter.Eq(s => s.StudentID, id);
            var student = await _studentCollection.Find(studentFilter).FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound(new { status = false, message = "Không tìm thấy học sinh này." });
            }

            // Lấy danh sách học phí của học sinh
            var feeFilter = Builders<TuitionFeesModel>.Filter.Eq(t => t.StudentID, id);
            var tuitionFees = await _tuitionFeesModel.Find(feeFilter).ToListAsync();





            var viewModel = new StudentTuitionViewModel
            {
                Student = student,
                TuitionFees = tuitionFees

            };

            return View(viewModel);
        }


        [HttpGet("admin/attendance/by-month")]
        public async Task<IActionResult> GetAttendanceByMonth(string studentId, int year, int month)
        {
            var student = await _studentCollection.Find(s => s.StudentID == studentId).FirstOrDefaultAsync();
            if (student == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy học sinh." });
            }

            var startOfMonth = new DateTime(year, month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var attendanceFilter = Builders<Attendance>.Filter.And(
                Builders<Attendance>.Filter.Eq(a => a.StudentID, student.Id),
                Builders<Attendance>.Filter.Gte(a => a.Date, startOfMonth),
                Builders<Attendance>.Filter.Lte(a => a.Date, endOfMonth)
            );
            var attendanceRecords = await _attendance.Find(attendanceFilter).ToListAsync();

            var allDatesInMonth = Enumerable.Range(0, (endOfMonth - startOfMonth).Days + 1)
                .Select(offset => startOfMonth.AddDays(offset))
                .ToList();

            var allAttendanceInMonth = allDatesInMonth.Select(date =>
            {
                var record = attendanceRecords.FirstOrDefault(a => a.Date.ToLocalTime().Date == date.Date);
                return new
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    IsChecked = record?.IsChecked ?? false,
                    Status = record?.Status ?? "Chưa điểm danh",
                    AttendanceID = record?.AttendanceID ?? 0,
                    IsTuitionCalculated = record?.IsTuitionCalculated ?? false,
                };
            }).ToList();

            var attended = allAttendanceInMonth.Where(a => a.IsChecked).ToList();
            var missed = allAttendanceInMonth.Where(a => !a.IsChecked).ToList();

            return Ok(new
            {
                success = true,
                attended,
                missed
            });
        }


        [HttpPost("admin/tuition-fees/add-fee")]
        public async Task<IActionResult> AddFee(string studentId, decimal amount, string title, List<int> IdAttendance)
        {
            using (var session = await _mongoClient.StartSessionAsync()) // Bắt đầu giao dịch
            {
                session.StartTransaction(); // Bắt đầu một giao dịch

                try
                {
                    var student = await _studentCollection.Find(s => s.StudentID == studentId).FirstOrDefaultAsync();
                    if (student == null)
                    {
                        return NotFound(new { status = false, message = "Không tìm thấy học sinh." });
                    }

                    if (IdAttendance.Count() == 0)
                    {
                        return BadRequest(new { status = false, message = "Không có bản ghi nào được chọn." });
                    }

                    if (amount <= 0)
                    {
                        return BadRequest(new { status = false, message = "Số tiền không hợp lệ." });
                    }

                    if (string.IsNullOrEmpty(title))
                    {
                        return BadRequest(new { status = false, message = "Tiêu đề không được để trống." });
                    }
                    int updatedCount = 0;
                    // Cập nhật tất cả các bản ghi Attendance
                    foreach (var record in IdAttendance)
                    {
                        var attendance = await _attendance.Find(a => a.AttendanceID == record).FirstOrDefaultAsync();

                        if (attendance == null)
                        {
                            continue; // Bỏ qua nếu không tìm thấy bản ghi
                        }

                        if (attendance.IsTuitionCalculated)
                        {
                            continue;

                        }
                        var update = Builders<Attendance>.Update.Set(a => a.IsTuitionCalculated, true);
                        await _attendance.UpdateOneAsync(a => a.AttendanceID == record, update);
                        updatedCount++;
                    }
                    if (updatedCount == 0)
                    {
                        return BadRequest(new { status = false, message = "Tất cả các bản ghi đều đã được tính học phí." });
                    }
                    decimal totalAmount = amount * updatedCount;

                    var fee = new TuitionFeesModel
                    {
                        StudentID = studentId,
                        Date = DateTime.Now,
                        Status = "Đang chờ",
                        Amount = totalAmount,
                        Title = title,
                        TotalDaysAttended = updatedCount
                    };

                    // Thêm học phí vào collection
                    await _tuitionFeesModel.InsertOneAsync(session, fee); // Thực hiện chèn trong giao dịch

                    // Cam kết giao dịch
                    await session.CommitTransactionAsync();

                    return Ok(new { status = true, message = "Thêm học phí thành công." });
                }
                catch (Exception ex)
                {
                    // Nếu có lỗi, rollback giao dịch
                    await session.AbortTransactionAsync();
                    return BadRequest(new { status = false, message = "Lỗi không xác định: " + ex.Message });
                }
            }
        }





    }
}
