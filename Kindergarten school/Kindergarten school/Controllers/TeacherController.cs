using Kindergarten_school.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Kindergarten_school.Controllers
{
    public class TeacherController : Controller
    {
        private readonly IMongoCollection<TeacherModel> _teacherCollection;

        public TeacherController(IMongoDatabase database)
        {
            _teacherCollection = database.GetCollection<TeacherModel>("teachers");
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
    }
}
