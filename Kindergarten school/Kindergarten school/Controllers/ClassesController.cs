using Kindergarten_school.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Kindergarten_school.Controllers
{
    public class ClassesController : Controller
    {
        private readonly IMongoCollection<ClassModel> _classCollection;

        public ClassesController(IMongoDatabase database)
        {
            _classCollection = database.GetCollection<ClassModel>("classes");
        }

        // GET: Classes
        public IActionResult Index()
        {
            var classes = _classCollection.Find(c => true).ToList();
            return View(classes);
        }

        // GET: Classes/Details/5
        public IActionResult Details(string id)
        {
            var classData = _classCollection.Find(c => c.Id == id).FirstOrDefault();
            if (classData == null)
            {
                return NotFound();
            }
            return View(classData);
        }

        // GET: Classes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Classes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClassModel classModel)
        {
            if (ModelState.IsValid)
            {
                _classCollection.InsertOne(classModel);
                return RedirectToAction(nameof(Index));
            }
            return View(classModel);
        }

        // GET: Classes/Edit/5
        public IActionResult Edit(string id)
        {
            var classData = _classCollection.Find(c => c.Id == id).FirstOrDefault();
            if (classData == null)
            {
                return NotFound();
            }
            return View(classData);
        }

        // POST: Classes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, ClassModel updatedClass)
        {
            if (ModelState.IsValid)
            {
                _classCollection.ReplaceOne(c => c.Id == id, updatedClass);
                return RedirectToAction(nameof(Index));
            }
            return View(updatedClass);
        }

        // GET: Classes/Delete/5
        public IActionResult Delete(string id)
        {
            var classData = _classCollection.Find(c => c.Id == id).FirstOrDefault();
            if (classData == null)
            {
                return NotFound();
            }
            return View(classData);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            _classCollection.DeleteOne(c => c.Id == id);
            return RedirectToAction(nameof(Index));
        }
    }
}
