using Kindergarten_school.Extensions;
using Kindergarten_school.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace Kindergarten_school.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        public async Task<IActionResult> Index()
        {
            var schedules = await _scheduleService.GetAllSchedulesAsync();
            return View(schedules);
        }

        public IActionResult Create()
        {
            ViewBag.Subjects = new List<string> { "Toán", "Tiếng Việt", "Khoa học", "Lịch sử", "Địa lý", "Mỹ thuật", "Âm nhạc" };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ScheduleModel schedule)
        {
            if (ModelState.IsValid)
            {
                await _scheduleService.CreateScheduleAsync(schedule);
                return RedirectToAction(nameof(Index));
            }
            return View(schedule);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var schedule = await _scheduleService.GetScheduleByIdAsync(id);
            if (schedule == null)
                return NotFound();
            // Danh sách môn học (có thể lấy từ database)
            ViewBag.Subjects = new List<SelectListItem>
    {
        new SelectListItem { Value = "Math", Text = "Toán" },
        new SelectListItem { Value = "Science", Text = "Khoa học" },
        new SelectListItem { Value = "English", Text = "Tiếng Anh" },
        new SelectListItem { Value = "Art", Text = "Mỹ thuật" },
        new SelectListItem { Value = "Music", Text = "Âm nhạc" }
    };
            return View(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, ScheduleModel schedule)
        {
            if (ModelState.IsValid)
            {
                await _scheduleService.UpdateScheduleAsync(id, schedule);
                return RedirectToAction(nameof(Index));
            }
            return View(schedule);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var schedule = await _scheduleService.GetScheduleByIdAsync(id);
            if (schedule == null)
                return NotFound();
            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _scheduleService.DeleteScheduleAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
