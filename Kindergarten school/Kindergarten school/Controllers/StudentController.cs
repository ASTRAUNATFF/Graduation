using Kindergarten_school.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Kindergarten_school.Controllers
{
    public class StudentController : Controller
    {
        private readonly IMongoCollection<StudentModel> _studentCollection;

        // Constructor: Inject MongoDB Database
        public StudentController(IMongoDatabase database)
        {
            _studentCollection = database.GetCollection<StudentModel>("students");
        }

        // GET: /Student/Index
        public async Task<IActionResult> Index()
        {
            var students = await _studentCollection.Find(_ => true).ToListAsync();
            return View(students); // Render danh sách students
        }

        // GET: /Student/Create
        public IActionResult Create()
        {
            return View(); // Hiển thị form tạo Student
        }

        // POST: /Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentModel student)
        {
            if (ModelState.IsValid)
            {
                await _studentCollection.InsertOneAsync(student);
                return RedirectToAction(nameof(Index));
            }
            return View(student); // Nếu không hợp lệ, trả lại form với lỗi
        }

        // GET: /Student/Edit/{id}
        public async Task<IActionResult> Edit(string id)
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
        public async Task<IActionResult> Edit(string id, StudentModel student)
        {
            if (ModelState.IsValid)
            {
                var filter = Builders<StudentModel>.Filter.Eq(s => s.Id, id);
                var update = Builders<StudentModel>.Update
                    .Set(s => s.FirstName, student.FirstName)
                    .Set(s => s.LastName, student.LastName)
                    .Set(s => s.Age, student.Age)
                    .Set(s => s.ClassId, student.ClassId)
                    .Set(s => s.Address, student.Address)
                    .Set(s => s.EnrollmentDate, student.EnrollmentDate);

                await _studentCollection.UpdateOneAsync(filter, update);
                return RedirectToAction(nameof(Index));
            }
            return View(student); // Nếu lỗi, trả lại form chỉnh sửa
        }

        // GET: /Student/Delete/{id}
        public async Task<IActionResult> Delete(string id)
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
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _studentCollection.DeleteOneAsync(s => s.Id == id);
            return RedirectToAction(nameof(Index)); // Trở về danh sách
        }

        // GET: /Student/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            var student = await _studentCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (student == null)
            {
                return NotFound();
            }
            return View(student); // Hiển thị thông tin chi tiết
        }
    }
}
    