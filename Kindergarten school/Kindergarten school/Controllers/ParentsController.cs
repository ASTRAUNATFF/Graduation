using Kindergarten_school.DTO;
using Kindergarten_school.Hubs;
using Kindergarten_school.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Security.Claims;

namespace Kindergarten_school.Controllers
{
    public class ParentsController : Controller
    {
        private readonly IMongoCollection<Parent> _parentCollection;
        private readonly IMongoCollection<Attendance> _attendanceCollection;
        private readonly IMongoCollection<StudentModel> _studentCollection;
        private readonly IMongoCollection<Fee> _feeCollection;
        private readonly IMongoCollection<DailyreportsModel> _dailyreports;
        private readonly IMongoCollection<NotificationSchool> _notificationSchool;
        private readonly IMongoCollection<HealthReport> _healthReport;
        private readonly IMongoCollection<TeacherModel> _teacherCollection;
        private readonly IMongoCollection<ClassModel> _classCollection;
        private readonly IMongoCollection<ChatConversationModel> _conversations;
        private readonly IMongoCollection<ChatMessageModel> _messages;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly IMongoCollection<AccountModel> _accountsCollection;

        private readonly IMongoCollection<BehaviorChartModel> _behaviorChartModel;


        public ParentsController(IMongoDatabase database, IHubContext<ChatHub> hubContext)
        {
            _parentCollection = database.GetCollection<Parent>("parents");
            _attendanceCollection = database.GetCollection<Attendance>("attendances");
            _studentCollection = database.GetCollection<StudentModel>("students");
            _feeCollection = database.GetCollection<Fee>("fees");
            _dailyreports = database.GetCollection<DailyreportsModel>("dailyreports");
            _notificationSchool = database.GetCollection<NotificationSchool>("notificationSchool");
            _healthReport = database.GetCollection<HealthReport>("HealthReport");
            _teacherCollection = database.GetCollection<TeacherModel>("teachers");
            _classCollection = database.GetCollection<ClassModel>("classes");
            _conversations = database.GetCollection<ChatConversationModel>("ChatConversation");
            _messages = database.GetCollection<ChatMessageModel>("ChatMessage");
            _behaviorChartModel = database.GetCollection<BehaviorChartModel>("BehaviorChart");

            _chatHub = hubContext;





        }


        [HttpGet("profile")]
        public async Task<IActionResult> Index()
        {
            var parentId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;


            if (string.IsNullOrEmpty(parentId))
            {
                return RedirectToAction("Login", "Account");
            }
            if (!int.TryParse(parentId, out int parsedParentId))
            {
                return BadRequest("Không có ParentID");
            }
            var parent = await _parentCollection.Find(s => s.AccountID == parsedParentId.ToString()).FirstOrDefaultAsync();
            if (parent == null)
            {
                return NotFound();
            }

            return View(parent);
        }


        [HttpGet("profile/list-student")]
        public async Task<IActionResult> ListStudent()
        {
            var parentId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;


            if (string.IsNullOrEmpty(parentId))
            {
                return RedirectToAction("Login", "Account");
            }
            if (!int.TryParse(parentId, out int parsedParentId))
            {
                return BadRequest("Không có ParentID");
            }
            var parent = await _parentCollection.Find(s => s.AccountID == parsedParentId.ToString()).FirstOrDefaultAsync();
            if (parent == null)
            {
                return NotFound();
            }

            var listStudent = await _studentCollection.Find(x => x.ParentID == parent.ParentID).ToListAsync();

            return View(listStudent);
        }

        [HttpGet("profile/attendances/{id}")]
        public async Task<IActionResult> StudentAttendance(string id)
        {


            var listAttendance = await _attendanceCollection.Find(x => x.StudentID == id).ToListAsync();

            return View(listAttendance);
        }

        [HttpGet("profile/fees/{id}")]
        public async Task<IActionResult> StudentFees(string id)
        {

            var studentFees = await _feeCollection.Find(f => f.StudentId == id).ToListAsync();

            return View(studentFees);
        }

        [HttpGet("profile/learning-progress/{id}")]
        public async Task<IActionResult> LearningProgress(int id)
        {

            var studentFees = await _dailyreports.Find(f => f.StudentID == id).ToListAsync();

            return View(studentFees);
        }
        [HttpGet("parent/notifications/{parentId}")]
        public async Task<IActionResult> GetNotificationsByParent(string parentId)
        {
            try
            {
                // Lấy danh sách học sinh thuộc về phụ huynh
                var students = await _studentCollection.Find(s => s.ParentID == parentId).ToListAsync();

                if (students == null || !students.Any())
                {
                    return NotFound(new { message = "Không tìm thấy học sinh nào của phụ huynh này." });
                }

                // Lấy danh sách StudentID và ClassID
                var studentIds = students.Select(s => s.StudentID).ToList();
                var classIds = students.Select(s => s.ClassID).Distinct().ToList(); // Lấy danh sách lớp không trùng

                // Tìm tất cả các thông báo:
                // - Thông báo dành riêng cho học sinh
                // - Thông báo gửi đến lớp của học sinh
                // - Thông báo gửi đến tất cả phụ huynh/học sinh
                var notifications = await _notificationSchool
                    .Find(n => studentIds.Contains(n.studentId.ToString())
                            || classIds.Contains(n.classID.ToString())
                            || n.type == "ToAll")
                    .SortByDescending(n => n.createDate)
                    .ToListAsync();

                return View(notifications);
            }
            catch
            {
                return NoContent();
            }
        }

        [HttpGet("parent/health-report/{parentId}")]
        public async Task<IActionResult> GetNotificationsHealth(string parentId)
        {
            try
            {
                // Lấy danh sách học sinh thuộc về phụ huynh
                var students = await _studentCollection.Find(s => s.ParentID == parentId).ToListAsync();

                if (students == null || !students.Any())
                {
                    return NotFound(new { message = "Không tìm thấy học sinh nào của phụ huynh này." });
                }

                // Tạo danh sách ánh xạ StudentID -> StudentName
                var studentDict = students.ToDictionary(s => s.StudentID, s => $"{s.FirstName} {s.LastName}");

                // Lấy danh sách StudentID 
                var studentIds = studentDict.Keys.ToList();

                var notifications = await _healthReport
                    .Find(n => studentIds.Contains(n.studentId.ToString()))
                    .SortByDescending(n => n.createDate)
                    .ToListAsync();

                // Ánh xạ dữ liệu sang DTO
                var notificationDTOs = notifications.Select(n => new HealthNotificationDTO
                {
                    Id = n.Id,
                    studentId = n.studentId,
                    createDate = n.createDate,
                    HealthStatus = n.HealthStatus,
                    StudentName = studentDict.ContainsKey(n.studentId.ToString()) ? studentDict[n.studentId.ToString()] : "Unknown"
                }).ToList();

                return View(notificationDTOs);
            }
            catch
            {
                return NoContent();
            }
        }


        [HttpGet("parent/chat/{parentId}")]
        public async Task<IActionResult> ChatParent(string parentId)
        {

            // 1. Lấy danh sách học sinh thuộc về phụ huynh
            var students = await _studentCollection.Find(s => s.ParentID == parentId).ToListAsync();

            if (students == null || !students.Any())
            {
                return NotFound(new { message = "Không tìm thấy học sinh nào của phụ huynh này." });
            }

            // 2. Lấy danh sách ClassID từ học sinh
            var classIds = students.Select(s => s.ClassID).Distinct().ToList();

            // 3. Lấy danh sách lớp học
            var classes = await _classCollection.Find(c => classIds.Contains(c.ClassID)).ToListAsync();

            // 4. Lấy danh sách TeacherID từ lớp học
            var teacherIds = classes.Select(c => c.TeacherID).Where(t => t != null).Distinct().ToList();

            // 5. Lấy danh sách giáo viên từ TeacherID
            var teachers = await _teacherCollection.Find(t => teacherIds.Contains(t.TeacherID.ToString())).ToListAsync();

            ViewBag.ParentId = parentId;
            return View(teachers);
        }
        [HttpGet("/parent/conversation/{parentId}/{teacherId}")]
        public async Task<string> GetOrCreateConversationAsync(string parentId, string teacherId)
        {
            var filter = Builders<ChatConversationModel>.Filter.All(c => c.Participants, new List<string> { parentId, teacherId });

            var conversation = await _conversations.Find(filter).FirstOrDefaultAsync();

            if (conversation == null)
            {
                conversation = new ChatConversationModel { Participants = new List<string> { parentId, teacherId } };
                await _conversations.InsertOneAsync(conversation);
            }

            return conversation.Id!;
        }

        // 2. Gửi tin nhắn
        [HttpPost("/parent/send")]
        public async Task<IActionResult> SendMessageAsync(string conversationId, string senderId, string message)
        {
            var chatMessage = new ChatMessageModel
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _messages.InsertOneAsync(chatMessage);
            await _chatHub.Clients.Group(conversationId).SendAsync("ReceiveMessageChat", senderId, message);
            return Ok("Thành công");

        }
        // 3. Lấy tin nhắn của một cuộc hội thoại
        [HttpGet("/parent/messages/{conversationId}")]
        public async Task<IActionResult> GetMessages(string conversationId)
        {
            var messages = await _messages.Find(m => m.ConversationId == conversationId)
                                  .Sort(Builders<ChatMessageModel>.Sort.Ascending(m => m.Timestamp))
                                  .ToListAsync();
            return Ok(messages);
        }
        [HttpGet("profile/behavior/{studentID}")]
        public async Task<IActionResult> GetBehavior(int studentID)
        {
            // Convert studentID sang int


            // Lấy danh sách behavior của học sinh đó
            var behaviorList = await _behaviorChartModel
                .Find(b => b.StudenId == studentID)
                .ToListAsync();

            // Lấy thông tin học sinh
            var student = await _studentCollection.Find(s => s.StudentID == studentID.ToString()).FirstOrDefaultAsync();
            if (student == null)
            {
                return NotFound(new { message = "Không tìm thấy học sinh." });
            }

            // Lấy thông tin lớp
            var classObj = await _classCollection.Find(c => c.ClassID == student.ClassID).FirstOrDefaultAsync();

            // Chuẩn bị kết quả
            var result = behaviorList.Select(behavior => new BehaviorDisplayViewModel
            {
                StudentName = $"{student.FirstName} {student.LastName}",
                ClassName = classObj?.ClassName ?? "Không rõ",
                Title = behavior.Title,
                Decription = behavior.Decription,
                CreateDate = behavior.CreateDate
            }).ToList();

            return View(result);
        }

    }
}
