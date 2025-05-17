using Kindergarten_school.Hubs;
using Kindergarten_school.Models;
//using Kindergarten_school.ViewModels.NotificationViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;



namespace Kindergarten_school.Controllers
{

    
    public class AdminNotificationController : Controller
    {
        private readonly IMongoCollection<Parent> _parentCollection;
        private readonly IMongoCollection<StudentModel> _studentCollection;
        private readonly IMongoCollection<ClassModel> _classCollection;
        private readonly IMongoCollection<DailyreportsModel> _dailyreports;
        private readonly IMongoCollection<NotificationSchool> _notificationSchool;
        private readonly IMongoCollection<Models.HealthReport> _healthReport;
        private readonly IHubContext<SchoolHub> _hubContext;
        private readonly IHubContext<HealthReportHub> _hubContextHealthReport;


        public AdminNotificationController(IMongoDatabase database, IHubContext<SchoolHub> hubContext, IHubContext<HealthReportHub> hubContextHealthReport)
        {
            _parentCollection = database.GetCollection<Parent>("parents");
            _studentCollection = database.GetCollection<StudentModel>("students");
            _dailyreports = database.GetCollection<DailyreportsModel>("dailyreports");
            _classCollection = database.GetCollection<ClassModel>("classes");
            _notificationSchool = database.GetCollection<NotificationSchool>("notificationSchool");
            _healthReport = database.GetCollection<Models.HealthReport>("HealthReport");
            _hubContext = hubContext;
            _hubContextHealthReport = hubContextHealthReport;




        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.ListClass = await _classCollection.Find(_ => true).ToListAsync();
            ViewBag.ListStudent = await _studentCollection.Find(_ => true).ToListAsync();

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> HealthReport()
        {
            ViewBag.ListStudent = await _studentCollection.Find(_ => true).ToListAsync();

            return View();
        }

        [HttpPost("admin/notification/create")]
        public async Task<IActionResult> CreateNotification(NotificationSchoolViewModels model)
        {
            try
            {
                NotificationSchool newNotification = new NotificationSchool
                {
                    type = model.type,
                    content = model.content,
                    createDate = DateTime.Now
                };

                switch (model.type)
                {
                    case "ToClass":
                        newNotification.classID = model.classId ?? 0;

                        break;
                    case "ToStudent":
                        newNotification.studentId = model.studentId ?? 0;
                        break;
                    case "ToAll":
                    default:
                        break;
                }

                await _notificationSchool.InsertOneAsync(newNotification);

                switch (model.type)
                {
                    case "ToClass":
                        await _hubContext.Clients.Group("class_" + newNotification.classID).SendAsync("SendMessageToClass", model.content, newNotification.createDate);

                        break;
                    case "ToStudent":
                        await _hubContext.Clients.Group("student_" + newNotification.studentId).SendAsync("SendMessageToStudent", model.content, newNotification.createDate);

                        break;
                    case "ToAll":
                    default:
                        await _hubContext.Clients.All.SendAsync("SendMessageToAll", model.content, newNotification.createDate);

                        break;
                }




                return Json(new { message = "Thành công", newNotification });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Đã có lỗi xảy ra", error = ex.Message });
            }
        }

        [HttpPost("admin/health-report/create")]
        public async Task<IActionResult> HealthReportNotification(Models.HealthReport model)
        {
            try
            {
                model.createDate = DateTime.Now;

                var student = await _studentCollection.Find(s => s.StudentID == model.studentId.ToString()).FirstOrDefaultAsync();
                if (student == null)
                {
                    return BadRequest(new { message = "Không tìm thấy học sinh" });
                }

                await _healthReport.InsertOneAsync(model);

                await _hubContextHealthReport.Clients.Group("student_" + model.studentId).SendAsync("HealthReportToStudent", model.HealthStatus, model.createDate, student.FirstName + student.LastName);

                return Json(new { message = "Thành công", model });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Đã có lỗi xảy ra", error = ex.Message });
            }
        }

    }
}
