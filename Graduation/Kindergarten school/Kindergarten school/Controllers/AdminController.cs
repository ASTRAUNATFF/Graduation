using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Kindergarten_school.Models;


namespace Kindergarten_school.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly IMongoCollection<Parent> _parentsCollection;

        public AdminController(IMongoDatabase database)
        {
            // Truy xuất collection "parents" từ database MongoDB
            _parentsCollection = database.GetCollection<Parent>("parents");
        }

        // Lấy danh sách phụ huynh
        public async Task<IActionResult> ParentList()
        {
            var parents = await _parentsCollection.Find(_ => true).ToListAsync();
            return View(parents);
        }

        // Thêm phụ huynh mới
        [HttpPost]
        public async Task<IActionResult> AddParent(Parent parent)
        {
            if (ModelState.IsValid)
            {
                await _parentsCollection.InsertOneAsync(parent);
                return RedirectToAction("ParentList");
            }
            return View(parent);
        }

        // Xóa phụ huynh
        [HttpPost]
        public async Task<IActionResult> DeleteParent(string id)
        {
            await _parentsCollection.DeleteOneAsync(p => p.Id == id);
            return RedirectToAction("ParentList");
        }
    }
}