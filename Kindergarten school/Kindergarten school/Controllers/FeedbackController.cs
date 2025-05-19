using Kindergarten_school.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kindergarten_school.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IMongoCollection<FeedbackModel> _feedbackCollection;
        private readonly IMongoCollection<Parent> _parentsCollection;

        public FeedbackController(IMongoDatabase database)
        {
            _feedbackCollection = database.GetCollection<FeedbackModel>("feedbacks");
            _parentsCollection = database.GetCollection<Parent>("parents");
        }

        // Hiển thị danh sách feedbacks
        public IActionResult Index()
        {
            var feedbacks = _feedbackCollection.Find(f => true).SortByDescending(f => f.CreateAt).ToList();
            return View(feedbacks);
        }

        // Hiển thị form thêm feedback
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý thêm feedback
        [HttpPost]
        public async Task<IActionResult> Create(FeedbackModel feedback)
        {
            if (!ModelState.IsValid)
            {
                return View(feedback);
            }

            // Kiểm tra số điện thoại có tồn tại trong hệ thống không
            var parent = await _parentsCollection.Find(p => p.Phone == feedback.Phone).FirstOrDefaultAsync();
            if (parent == null)
            {
                ViewBag.Error = "Số điện thoại không tồn tại trong hệ thống!";
                return View(feedback);
            }

            // Gán ParentID tự động
            feedback.ParentID = parent.ParentID;
            feedback.CreateAt = DateTime.UtcNow;

            // Lưu vào MongoDB
            await _feedbackCollection.InsertOneAsync(feedback);

            return RedirectToAction("Index");
        }


    }
}
