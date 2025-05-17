using Kindergarten_school.Hubs;
using Kindergarten_school.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace Kindergarten_school.Controllers
{
    public class TeacherController : Controller
    {
        private readonly IMongoCollection<TeacherModel> _teacherCollection;
        private readonly IMongoCollection<Parent> _parentCollection;
        private readonly IMongoCollection<ClassModel> _classCollection;
        private readonly IMongoCollection<ChatConversationModel> _conversations;
        private readonly IMongoCollection<ChatMessageModel> _messages;
        private readonly IMongoCollection<StudentModel> _studentCollection;
        private readonly IMongoCollection<BehaviorChartModel> _behaviorChartModel;
        private readonly IHubContext<ChatHub> _chatHub;

        public TeacherController(IMongoDatabase database, IHubContext<ChatHub> hubContext)
        {

            _teacherCollection = database.GetCollection<TeacherModel>("teachers");
            _parentCollection = database.GetCollection<Parent>("parents");

            _classCollection = database.GetCollection<ClassModel>("classes");
            _conversations = database.GetCollection<ChatConversationModel>("ChatConversation");
            _messages = database.GetCollection<ChatMessageModel>("ChatMessage");
            _studentCollection = database.GetCollection<StudentModel>("students");
            _behaviorChartModel = database.GetCollection<BehaviorChartModel>("BehaviorChart");
            _chatHub = hubContext;

        }


        // GET: /Teacher/Index
        public async Task<IActionResult> Index()
        {
            var teachers = await _teacherCollection.Find(_ => true).ToListAsync();
            return View(teachers);
        }

        // GET: /Teacher/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Teacher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherModel teacher)
        {
            if (ModelState.IsValid)
            {
                await _teacherCollection.InsertOneAsync(teacher);
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        // GET: /Teacher/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var teacher = await _teacherCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: /Teacher/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TeacherModel teacher)
        {
            if (ModelState.IsValid)
            {
                var filter = Builders<TeacherModel>.Filter.Eq(t => t.Id, id);
                var update = Builders<TeacherModel>.Update
                    .Set(t => t.FirstName, teacher.FirstName)
                    .Set(t => t.LastName, teacher.LastName)
                    .Set(t => t.Subject, teacher.Subject)
                    .Set(t => t.Phone, teacher.Phone)
                    .Set(t => t.HireDate, teacher.HireDate)
                    .Set(t => t.Address, teacher.Address)
                    .Set(t => t.TeacherID, teacher.TeacherID);
                await _teacherCollection.UpdateOneAsync(filter, update);
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        // GET: /Teacher/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            var teacher = await _teacherCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: /Teacher/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _teacherCollection.DeleteOneAsync(t => t.Id == id);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Teacher/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            var teacher = await _teacherCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        [HttpGet("teacher/chat/{id}")]
        public async Task<IActionResult> ChatTeacher(string id)
        {
            // 1. Tìm giáo viên theo ID
            var teacher = await _teacherCollection.Find(t => t.AccountID == id).FirstOrDefaultAsync();
            if (teacher == null)
            {
                return NotFound(new { message = "Không tìm thấy giáo viên." });
            }

            // 2. Lấy danh sách lớp mà giáo viên này dạy
            var classList = await _classCollection.Find(c => c.TeacherID == teacher.TeacherID.ToString()).ToListAsync();
            var classIds = classList.Select(c => c.ClassID).ToList();

            if (!classIds.Any())
            {
                return NotFound(new { message = "Giáo viên này chưa có lớp nào." });
            }

            // 3. Lấy danh sách học sinh thuộc các lớp này
            var classIdsString = classIds.Select(id => id.ToString()).ToList();
            var students = await _studentCollection
                .Find(s => classIdsString.Contains(s.ClassID.ToString()))
                .ToListAsync();

            var parentIds = students.Select(s => s.ParentID).Distinct().ToList();

            if (!parentIds.Any())
            {
                return NotFound(new { message = "Không có phụ huynh nào liên quan đến giáo viên này." });
            }

            // 4. Lấy danh sách phụ huynh
            var parents = await _parentCollection.Find(p => parentIds.Contains(p.ParentID)).ToListAsync();
            ViewBag.TeacherId = teacher.TeacherID;

            return View(parents);
        }


        [HttpGet("/teacher/conversation/{parentId}/{teacherId}")]
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
        [HttpPost("/teacher/send")]
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
        [HttpGet("/teacher/messages/{conversationId}")]
        public async Task<IActionResult> GetMessages(string conversationId)
        {
            var messages = await _messages.Find(m => m.ConversationId == conversationId)
                                  .Sort(Builders<ChatMessageModel>.Sort.Ascending(m => m.Timestamp))
                                  .ToListAsync();
            return Ok(messages);
        }

        [HttpGet("teacher/behavior/{teacherId}")]
        public async Task<IActionResult> GetBehavior(string teacherId)
        {
            var classList = await _classCollection.Find(c => c.TeacherID == teacherId).ToListAsync();
            var classIds = classList.Select(c => c.ClassID).ToList();

            if (!classIds.Any())
            {
                return NotFound(new { message = "Giáo viên này chưa có lớp nào." });
            }

            // Lấy danh sách học sinh thuộc các lớp
            var classIdsString = classIds.Select(id => id.ToString()).ToList();
            var students = await _studentCollection
                .Find(s => classIdsString.Contains(s.ClassID.ToString()))
                .ToListAsync();

            var studentIds = students.Select(s => s.StudentID).ToList();

            // Lấy danh sách phiếu bé ngoan (behavior) của các học sinh
            var behaviorList = await _behaviorChartModel
                .Find(b => studentIds.Contains(b.StudenId.ToString()))
                .ToListAsync();

            // Ghép thông tin học sinh và lớp vào phiếu
            var result = from behavior in behaviorList
                         join student in students on behavior.StudenId.ToString() equals student.StudentID
                         join cls in classList on student.ClassID equals cls.ClassID
                         select new BehaviorDisplayViewModel
                         {
                             StudentName = student.FirstName + student.LastName,
                             ClassName = cls.ClassName,
                             Title = behavior.Title,
                             Decription = behavior.Decription,
                             CreateDate = behavior.CreateDate
                         };

            return View(result.ToList());
        }


        [HttpGet("teacher/add-behavior/{teacherId}")]
        public async Task<IActionResult> AddBehavior(string teacherId)
        {
            // Lấy danh sách lớp của giáo viên đó
            var classes = await _classCollection
                .Find(c => c.TeacherID == teacherId)
                .ToListAsync();

            var classIds = classes.Select(c => c.ClassID.ToString()).ToList();


            var viewModel = new BehaviorCreateViewModel
            {
                Classes = classes
            };

            return View(viewModel);
        }


        [HttpPost("teacher/add-behavior/{teacherId}")]
        public async Task<IActionResult> AddBehavior(string teacherId, BehaviorCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var behavior = new BehaviorChartModel
                {
                    StudenId = int.Parse(model.SelectedStudentId),
                    ClassId = int.Parse(model.SelectedClassId),
                    Title = model.Title,
                    Decription = model.Decription,
                    CreateDate = model.CreateDate
                };

                await _behaviorChartModel.InsertOneAsync(behavior);

                return RedirectToAction("GetBehavior", new { teacherId });
            }

            // Nếu lỗi, nạp lại đúng dữ liệu của giáo viên đó
            var classes = await _classCollection
                .Find(c => c.TeacherID == teacherId)
                .ToListAsync();

            var classIds = classes.Select(c => c.ClassID.ToString()).ToList();

            var students = await _studentCollection
                .Find(s => classIds.Contains(s.ClassID.ToString()))
                .ToListAsync();

            model.Classes = classes;
            model.Students = students;

            return View(model);
        }
        [HttpGet("teacher/get-students-by-class/{classId}")]
        public async Task<IActionResult> GetStudentsByClass(int classId)
        {
            try
            {

                var students = await _studentCollection
                    .Find(s => s.ClassID.Equals(classId)) // dùng .Equals để an toàn hơn
                    .ToListAsync();


                var result = students.Select(s => new
                {
                    s.StudentID,
                    FullName = $"{s.FirstName} {s.LastName}"
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });

            }

        }
    }
}
