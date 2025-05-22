using Kindergarten_school.Hubs;
using Kindergarten_school.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Kindergarten_school.Controllers
{
    public class DailyreportsController : Controller
    {
        private readonly IMongoCollection<Parent> _parentCollection;
        private readonly IMongoCollection<StudentModel> _studentCollection;
        private readonly IMongoCollection<ClassModel> _classCollection;
        private readonly IMongoCollection<DailyreportsModel> _dailyreports;
        private readonly IHubContext<ReportsHub> _hubContext;


        public DailyreportsController(IMongoDatabase database, IHubContext<ReportsHub> hubContext)
        {
            _parentCollection = database.GetCollection<Parent>("parents");
            _studentCollection = database.GetCollection<StudentModel>("students");
            _dailyreports = database.GetCollection<DailyreportsModel>("dailyreports");
            _classCollection = database.GetCollection<ClassModel>("classes");

            _hubContext = hubContext;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.ListClass = await _classCollection.Find(_ => true).ToListAsync();
            ViewBag.ListStudent = await _studentCollection.Find(_ => true).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(DailyreportsModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ListClass = await _classCollection.Find(_ => true).ToListAsync();
                ViewBag.ListStudent = await _studentCollection.Find(_ => true).ToListAsync();

                return View(model);
            }

            var lastReport = await _dailyreports.Find(_ => true)
                .SortByDescending(r => r.ReportID)
                .Limit(1)
                .FirstOrDefaultAsync();

            model.ReportID = (lastReport != null) ? lastReport.ReportID + 1 : 1;
            model.ReportDate = DateTime.Now;
            await _dailyreports.InsertOneAsync(model);
            await _hubContext.Clients.All.SendAsync("ReceiveMessageUser", model.ReportDate, model.Activities, model.Meal, model.NapTime, model.Description);
            return RedirectToAction("Index");
        }
    }
}