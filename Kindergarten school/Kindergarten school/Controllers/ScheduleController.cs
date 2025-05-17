using Kindergarten_school.Models;
using Kindergarten_school.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kindergarten_school.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        private readonly Dictionary<string, string> _activityMapping;
        private readonly Dictionary<int, string> _timeSlotMapping;
        private readonly IMongoCollection<TeacherModel> _teacherCollection;

        public ScheduleController(IScheduleService scheduleService, IMongoDatabase database)
        {
            _scheduleService = scheduleService;
            _teacherCollection = database.GetCollection<TeacherModel>("teachers");

            // 🔹 Khởi tạo danh sách hoạt động
            _activityMapping = new Dictionary<string, string>
            {
                { "morning_exercise", "Thể dục buổi sáng" },
                { "story_time", "Kể chuyện" },
                { "art_craft", "Thủ công & Nghệ thuật" },
                { "music_dance", "Âm nhạc & Khiêu vũ" },
                { "lunch", "Giờ ăn trưa" },
                { "nap_time", "Giấc ngủ trưa" },
                { "outdoor_play", "Vui chơi ngoài trời" },
                { "science_exploration", "Khám phá khoa học" }
            };

            // 🔹 Thêm danh sách TimeSlot
            _timeSlotMapping = new Dictionary<int, string>
            {
                { 1, "07:00 - 07:30" },
                { 2, "08:30 - 09:00" },
                { 3, "09:10 - 09:40" },
                { 4, "09:40 - 10:10" },
                { 5, "10:10 - 10:20" },
                { 6, "10:20 - 10:50" },
                { 7, "10:50 - 11:50" },
                { 8, "11:50 - 14:00" },
                { 9, "14:00 - 14:50" },
                { 10, "14:50 - 15:10" }
            };
        }

        public async Task<IActionResult> Index(string? classId)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            ViewBag.UserRole = userRole ?? "";

            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
            {
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "Không thể xác định vai trò của người dùng"
                });

            }

            List<ScheduleModel> schedules;

            switch (userRole)
            {
                case "admin":
                    var allClasses = await _scheduleService.GetAllClassesAsync(); // Lấy danh sách lớp
                    ViewBag.Classes = allClasses;

                    if (!string.IsNullOrEmpty(classId))
                    {
                        schedules = await _scheduleService.GetSchedulesByClassAsync(classId);
                    }
                    else
                    {
                        schedules = new List<ScheduleModel>(); // Chưa chọn lớp => chưa hiển thị lịch
                    }
                    break;


                case "teacher":
                    // Lấy class ID từ TeacherId (userId)
                    var teacherClassId = await _scheduleService.GetClassIdByTeacherIdAsync(userId);
                    schedules = await _scheduleService.GetSchedulesByClassIdAsync(teacherClassId);
                    break;

                case "parent":
                    // Lấy student ID từ parent ID (userId)
                    var studentId = await _scheduleService.GetStudentIdByParentIdAsync(userId);
                    if (studentId == null)
                        return View("Error", new ErrorViewModel
                        {
                            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                            Message = "Không tìm thấy phụ huynh liên kết với học sinh này"
                            
                        });

                    var studentClassId = await _scheduleService.GetClassIdByStudentIdAsync(studentId);
                    schedules = await _scheduleService.GetSchedulesByClassIdAsync(studentClassId);
                    break;

                default:
                    return View("Error", new ErrorViewModel
                    {
                        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                        Message = "Vai Trò người dùng không hợp lệ"
                        
                    });
            }

            if (schedules == null || !schedules.Any())
            {
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    Message = "Không có dữ liệu thời khóa biểu phù hợp"
                    
                });
            }

            Console.WriteLine($"Controller nhận được {schedules.Count} lịch học.");

            ViewBag.Activities = _activityMapping.Values.ToList();
            ViewBag.TimeSlots = _timeSlotMapping.Values.ToList();

            return View(schedules);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> SaveSchedule(List<ScheduleUpdateModel> updates)
        {
            if (updates == null || !updates.Any())
            {
                TempData["Error"] = "Không có dữ liệu cập nhật.";
                return RedirectToAction("Index");
            }

            foreach (var update in updates)
            {
                var schedule = await _scheduleService.GetScheduleByIdAsync(update.Id);
                if (schedule != null)
                {
                    schedule.Activity = update.NewActivity;
                    await _scheduleService.UpdateScheduleAsync(schedule.Id, schedule);
                }
            }

            TempData["Success"] = "Lưu thời khóa biểu thành công!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Teachers = await GetTeacherSelectListAsync();
            ViewBag.TimeSlots = GetTimeSlotSelectList();

            // Khi chưa có dữ liệu lớp/ngày/slot thì chưa lọc được danh sách môn
            ViewBag.Activities = GetActivitySelectList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ScheduleCreateModel model)
        {
            if (ModelState.IsValid)
            {
                var parts = model.TeacherSelection.Split('|');
                model.TeacherID = parts[0];
                model.ClassID = parts[1];

                var teacher = await _teacherCollection.Find(t => t.TeacherID == model.TeacherID).FirstOrDefaultAsync();
                var allSchedules = await _scheduleService.GetAllSchedulesAsync();

                // 🔸 Lọc lịch của lớp trong cùng ngày
                var sameDaySchedules = allSchedules
                    .Where(s => s.ClassId == model.ClassID && s.DayOfWeek == model.DayOfWeek)
                    .ToList();

                // 🔸 Kiểm tra duplicate môn trong ngày
                bool isDuplicateActivity = sameDaySchedules.Any(s => s.Activity == model.Activity);
                if (isDuplicateActivity)
                {
                    ModelState.AddModelError("", "Môn học này đã được xếp trong ngày này rồi.");
                    ViewBag.Teachers = await GetTeacherSelectListAsync();
                    ViewBag.TimeSlots = GetTimeSlotSelectList();
                    ViewBag.Activities = GetActivitySelectList(model.Activity, sameDaySchedules, model.TimeSlot);
                    return View(model);
                }

                // 🔸 Kiểm tra Thể dục buổi sáng chỉ ở khung 1
                if (model.Activity == "morning_exercise" && model.TimeSlot != 1)
                {
                    ModelState.AddModelError("", "Thể dục buổi sáng chỉ được chọn vào khung giờ 07:00 - 07:30.");
                    ViewBag.Teachers = await GetTeacherSelectListAsync();
                    ViewBag.TimeSlots = GetTimeSlotSelectList();
                    ViewBag.Activities = GetActivitySelectList(model.Activity, sameDaySchedules, model.TimeSlot);
                    return View(model);
                }

                var newSchedule = new ScheduleModel
                {
                    TeacherId = model.TeacherID,
                    ClassId = model.ClassID,
                    TeacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "Unknown",
                    TimeSlot = model.TimeSlot,
                    Activity = model.Activity,
                    DayOfWeek = model.DayOfWeek
                };

                await _scheduleService.CreateScheduleAsync(newSchedule);
                return RedirectToAction("Index");
            }

            ViewBag.Teachers = await GetTeacherSelectListAsync();
            ViewBag.TimeSlots = GetTimeSlotSelectList();
            ViewBag.Activities = GetActivitySelectList();
            return View(model);
        }


        public async Task<IActionResult> Edit(string id)
        {
            var schedule = await _scheduleService.GetScheduleByIdAsync(id);
            if (schedule == null)
            {
                return NotFound("Không tìm thấy lịch học.");
            }

            var allSchedules = await _scheduleService.GetAllSchedulesAsync();
            var sameDaySchedules = allSchedules
                .Where(s => s.ClassId == schedule.ClassId && s.DayOfWeek == schedule.DayOfWeek)
                .ToList();

            ViewBag.Activities = GetActivitySelectList(schedule.Activity, sameDaySchedules, schedule.TimeSlot);
            ViewBag.TimeSlots = GetTimeSlotSelectList(schedule.TimeSlot);
            return View(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ScheduleModel model)
        {
            if (ModelState.IsValid)
            {
                var allSchedules = await _scheduleService.GetAllSchedulesAsync();

                // 🔸 Lọc lịch cùng lớp và ngày, trừ chính bản thân lịch này (đang sửa)
                var sameDaySchedules = allSchedules
                    .Where(s => s.ClassId == model.ClassId && s.DayOfWeek == model.DayOfWeek && s.Id != model.Id)
                    .ToList();

                // 🔸 Check duplicate activity trong cùng ngày
                bool isDuplicateActivity = sameDaySchedules.Any(s => s.Activity == model.Activity);
                if (isDuplicateActivity)
                {
                    ModelState.AddModelError("", "Môn học này đã được xếp trong ngày này rồi.");
                    ViewBag.TimeSlots = GetTimeSlotSelectList(model.TimeSlot);
                    ViewBag.Activities = GetActivitySelectList(model.Activity, sameDaySchedules, model.TimeSlot);
                    return View(model);
                }

                // 🔸 Check “Thể dục buổi sáng” chỉ ở slot 1
                if (model.Activity == "morning_exercise" && model.TimeSlot != 1)
                {
                    ModelState.AddModelError("", "Thể dục buổi sáng chỉ được chọn vào khung giờ 07:00 - 07:30.");
                    ViewBag.TimeSlots = GetTimeSlotSelectList(model.TimeSlot);
                    ViewBag.Activities = GetActivitySelectList(model.Activity, sameDaySchedules, model.TimeSlot);
                    return View(model);
                }

                // 🔸 Update
                await _scheduleService.UpdateScheduleAsync(model.Id, model);
                return RedirectToAction("Index");
            }

            // Nếu có lỗi thì reload lại ViewBag
            var schedules = await _scheduleService.GetAllSchedulesAsync();
            var sameDay = schedules
                .Where(s => s.ClassId == model.ClassId && s.DayOfWeek == model.DayOfWeek && s.Id != model.Id)
                .ToList();

            ViewBag.TimeSlots = GetTimeSlotSelectList(model.TimeSlot);
            ViewBag.Activities = GetActivitySelectList(model.Activity, sameDay, model.TimeSlot);
            return View(model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var schedule = await _scheduleService.GetScheduleByIdAsync(id);
            if (schedule == null)
            {
                return NotFound("Không tìm thấy lịch học.");
            }

            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _scheduleService.DeleteScheduleAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa lịch học: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ViewByClass(string classId)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            List<ScheduleModel> schedule;
            var schedules = await _scheduleService.GetSchedulesByClassAsync(classId);

            if (schedules == null || !schedules.Any())
            {
                TempData["Error"] = "Chưa có thời khóa biểu cho lớp này.";
                return RedirectToAction("Index", "Class"); // hoặc view riêng
            }

            ViewBag.ClassId = classId;
            ViewBag.TimeSlots = _timeSlotMapping;
            ViewBag.Activities = _activityMapping;

            return View("ViewByClass", schedules);
        }





        private IEnumerable<SelectListItem> GetActivitySelectList(string? selectedActivity = "", List<ScheduleModel>? daySchedules = null, int currentSlot = 0)
        {
            var usedActivities = daySchedules?
                .Where(s => s.TimeSlot != currentSlot) // Bỏ qua slot hiện tại nếu đang edit
                .Select(s => s.Activity)
                .ToHashSet() ?? new HashSet<string>();

            return _activityMapping
                .Where(item => !usedActivities.Contains(item.Key) || item.Key == selectedActivity)
                .Select(item => new SelectListItem
                {
                    Value = item.Key,
                    Text = item.Value,
                    Selected = item.Key == selectedActivity
                });
        }

        private IEnumerable<SelectListItem> GetTimeSlotSelectList(int? selectedTimeSlot = null)
        {
            return _timeSlotMapping.Select(item => new SelectListItem
            {
                Value = item.Key.ToString(),
                Text = item.Value,
                Selected = item.Key == selectedTimeSlot
            });
        }

        private int GetTimeSlotIndex(string timeSlotLabel)
        {
            return _timeSlotMapping.FirstOrDefault(x => x.Value == timeSlotLabel).Key;
        }


        private async Task<List<SelectListItem>> GetTeacherSelectListAsync()
        {
            var teachers = await _teacherCollection.Find(_ => true).ToListAsync();

            if (teachers == null || !teachers.Any())
            {
                // Debug thử ở đây
                return new List<SelectListItem>(); // tránh null
            }

            return teachers.Select(t => new SelectListItem
            {
                Value = $"{t.TeacherID}|{t.ClassID}",
                Text = $"{t.FirstName} {t.LastName}"
            }).ToList();
        }
    }
}
