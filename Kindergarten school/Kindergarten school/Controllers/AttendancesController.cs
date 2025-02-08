using Kindergarten_school.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Kindergarten_school.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly IMongoCollection<Attendance> _attendanceCollection;

        public AttendanceController(IMongoDatabase database)
        {
            _attendanceCollection = database.GetCollection<Attendance>("attendances");
        }

        // Hiển thị danh sách điểm danh
        public IActionResult Index()
        {
            var attendances = _attendanceCollection.Find(_ => true).ToList();
            return View(attendances);
        }

        // Hiển thị form thêm điểm danh
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý thêm điểm danh
        [HttpPost]
        public IActionResult Create(Attendance attendance)
        {
            if (ModelState.IsValid)
            {
                _attendanceCollection.InsertOne(attendance);
                return RedirectToAction("Index");
            }
            return View(attendance);
        }
    }
}
