using Kindergarten_school.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace Kindergarten_school.Controllers
{
    public class NotificationController : Controller
    {
        private readonly IMongoCollection<NotificationModel> _notificationCollection;
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationController(IMongoDatabase database, IHubContext<NotificationHub> hubContext)
        {
            _notificationCollection = database.GetCollection<NotificationModel>("notifications");
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> NotifyScheduleChange()
        {
            string message = "Thời khóa biểu đã được thay đổi, vui lòng kiểm tra!";
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
            return Ok(new { success = true, message = "Notification sent successfully!" });
        }


        // Action: Danh sách thông báo
        public IActionResult Index()
        {
            var notifications = _notificationCollection.Find(_ => true).ToList();
            return View(notifications);
        }

        // Action: Tạo thông báo mới (GET)
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NotificationModel notification)
        {
            if (ModelState.IsValid)
            {
                // Lưu thông báo vào MongoDB
                notification.CreatedAt = DateTime.Now;
                await _notificationCollection.InsertOneAsync(notification);

                // Gửi thông báo đến tất cả các client qua SignalR
                string message = notification.Message; // Hoặc một thông báo tùy chỉnh
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);

                // Chuyển hướng về trang danh sách thông báo
                return RedirectToAction(nameof(Index));
            }
            return View(notification);
        }


        // Action: Chi tiết thông báo
        public IActionResult Details(string id)
        {
            var notification = _notificationCollection.Find(n => n.Id == id).FirstOrDefault();
            if (notification == null)
            {
                return NotFound();
            }
            return View(notification);
        }

        // Action: Xóa thông báo
        public IActionResult Delete(string id)
        {
            var notification = _notificationCollection.Find(n => n.Id == id).FirstOrDefault();
            if (notification == null)
            {
                return NotFound();
            }
            return View(notification);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            _notificationCollection.DeleteOne(n => n.Id == id);
            return RedirectToAction(nameof(Index));
        }
    }
}
