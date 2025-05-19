using Kindergarten_school.Models;
using Kindergarten_school.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;

namespace Kindergarten_school.Controllers
{
    public class AdminController : Controller
    {
        private readonly IMongoCollection<ClassModel> _classCollection;
        private readonly IMongoCollection<StudentModel> _studentCollection;
        private readonly IMongoCollection<TeacherModel> _teacherCollection;
       
        public AdminController(IMongoDatabase database)
        {
            _studentCollection = database.GetCollection<StudentModel>("students");
            _classCollection = database.GetCollection<ClassModel>("classes");
            _teacherCollection = database.GetCollection<TeacherModel>("teachers");
        }



        [Authorize(Roles = "admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [Authorize(Roles = "teacher")]
        public IActionResult TeacherHomeLand()
        {
            return View();
        }

        // Class Controller
        #region Class Controller
        //Class Controller
        public IActionResult ClassDashboard()
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            List<ClassModel> classes;

            if (userRole == "admin")
            {
                classes = _classCollection.Find(c => true).ToList();
            }
            else if (userRole == "teacher")
            {
                classes = _classCollection.Find(c => c.TeacherID == userId).ToList();
            }
            else
            {
                return Unauthorized();
            }

            return View(classes);
        }

        // GET: Classes/Details/5
        public IActionResult ClassDetails(string id)
        {
            var classData = _classCollection.Find(c => c.ClassID == id).FirstOrDefault();
            if (classData == null) return NotFound();

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userRole != "admin" && classData.TeacherID != userId)
               return Forbid();

            var studentsInClass = _studentCollection.Find(s => s.ClassID == id).ToList();
            ViewBag.Students = studentsInClass;
            ViewBag.Class = classData;
            return View(studentsInClass);
        }

        // GET: Classes/Create
        public IActionResult ClassCreate()
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (userRole != "admin" && userRole != "teacher")
                return Unauthorized();

            return View();
        }

        // POST: Classes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClassCreate(ClassModel classModel)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userRole == "teacher")
            {
                classModel.TeacherID = userId;
            }

            // Gán ID mới nếu chưa có
            if (string.IsNullOrEmpty(classModel.ClassID))
            {
                classModel.ClassID = ObjectId.GenerateNewId().ToString();
            }

            try
            {
                _classCollection.InsertOne(classModel);
                Console.WriteLine("Lớp học được thêm thành công!");
                return RedirectToAction(nameof(ClassDashboard));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi thêm lớp: " + ex.Message);
                ModelState.AddModelError(string.Empty, "Không thể thêm lớp học. Vui lòng thử lại.");
            }

            return View(classModel);
        }

        // GET: Classes/Edit/5
        public IActionResult ClassEdit(string id)
        {
            var classData = _classCollection.Find(c => c.Id == id).FirstOrDefault();
            if (classData == null) return NotFound();

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userRole != "admin" && classData.TeacherID != userId)
                return Forbid();

            return View(classData);
        }

        // POST: Classes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClassEdit(string id, ClassModel updatedClass)
        {
            var classData = _classCollection.Find(c => c.Id == id).FirstOrDefault();
            if (classData == null) return NotFound();

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userRole != "admin" && classData.TeacherID != userId)
                return Forbid();

            // Đảm bảo giáo viên không sửa TeacherID
            if (userRole == "teacher")
                updatedClass.TeacherID = userId;

            if (ModelState.IsValid)
            {
                _classCollection.ReplaceOne(c => c.Id == id, updatedClass);
                return RedirectToAction(nameof(Index));
            }

            return View(updatedClass);
        }

        // GET: Classes/Delete/5
        public IActionResult ClassDelete(string id)
        {
            var classData = _classCollection.Find(c => c.Id == id).FirstOrDefault();
            if (classData == null) return NotFound();

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userRole != "admin" && classData.TeacherID != userId)
                return Forbid();

            return View(classData);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("ClassDeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public IActionResult ClassDeleteConfirmed(string id)
        {
            var classData = _classCollection.Find(c => c.Id == id).FirstOrDefault();
            if (classData == null) return NotFound();

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userRole != "admin" && classData.TeacherID != userId)
                return Forbid();

            _classCollection.DeleteOne(c => c.Id == id);
            return RedirectToAction(nameof(Index));
        }
        #endregion

        //Student Controller
        #region Student Controller
        public async Task<IActionResult> StudentDashboard()
        {
            var students = await _studentCollection.Find(_ => true).ToListAsync();
            return View(students); // Render danh sách students
        }

        // GET: /Student/Create
        public IActionResult StudentCreate()
        {
            return View(); // Hiển thị form tạo Student
        }

        // POST: /Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentCreate(StudentModel student)
        {
            if (ModelState.IsValid)
            {
                await _studentCollection.InsertOneAsync(student);
                return RedirectToAction(nameof(Index));
            }
            return View(student); // Nếu không hợp lệ, trả lại form với lỗi
        }

        // GET: /Student/Edit/{id}
        public async Task<IActionResult> StudentEdit(string id)
        {
            var student = await _studentCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (student == null)
            {
                return NotFound();
            }
            return View(student); // Hiển thị form chỉnh sửa
        }

        // POST: /Student/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentEdit(StudentModel student)
        {
            if (!ModelState.IsValid)
            {
                return View(student);
            }

            var filter = Builders<StudentModel>.Filter.Eq(s => s.Id, student.Id);
            var update = Builders<StudentModel>.Update
                .Set(s => s.FirstName, student.FirstName)
                .Set(s => s.LastName, student.LastName)
                .Set(s => s.Age, student.Age)
                .Set(s => s.ClassID, student.ClassID)
                .Set(s => s.Address, student.Address)
                .Set(s => s.EnrollmentDate, student.EnrollmentDate);

            var result = await _studentCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                Console.WriteLine("Không có bản ghi nào được cập nhật.");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Student/Delete/{id}
        public async Task<IActionResult> StudentDelete(string id)
        {
            var student = await _studentCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (student == null)
            {
                return NotFound();
            }
            return View(student); // Hiển thị xác nhận xóa
        }

        // POST: /Student/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentDeleteConfirmed(string id)
        {
            await _studentCollection.DeleteOneAsync(s => s.Id == id);
            return RedirectToAction(nameof(Index)); // Trở về danh sách
        }

        // GET: /Student/Details/{id}
        public async Task<IActionResult> StudentDetails(string id)
        {
            var student = await _studentCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (student == null)
            {
                return NotFound();
            }
            return View(student); // Hiển thị thông tin chi tiết
        }
        #endregion

        //Teacher Controller
        #region Teacher Controller
        public async Task<IActionResult> TeacherDashboard()
        {
            var teachers = await _teacherCollection.Find(_ => true).ToListAsync();
            return View(teachers);
        }

        // GET: /Teacher/Create
        public IActionResult TeacherCreate()
        {
            return View();
        }

        // POST: /Teacher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeacherCreate(TeacherModel teacher)
        {
            if (ModelState.IsValid)
            {
                await _teacherCollection.InsertOneAsync(teacher);
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        // GET: /Teacher/Edit/{id}
        public async Task<IActionResult> TeacherEdit(string id)
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
        public async Task<IActionResult> TeacherEdit(string id, TeacherModel teacher)
        {

            if (ModelState.IsValid)
            {
                var filter = Builders<TeacherModel>.Filter.Eq(t => t.Id, teacher.Id);
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
        public async Task<IActionResult> TeacherDelete(string id)
        {
            var teacher = await _teacherCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: /Teacher/Delete/{id}
        [HttpPost, ActionName("TeacherDeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TeacherDeleteConfirmed(string id)
        {
            await _teacherCollection.DeleteOneAsync(t => t.Id == id);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Teacher/Details/{id}
        public async Task<IActionResult> TeacherDetails(string id)
        {
            var teacher = await _teacherCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }
        #endregion

    }
}
