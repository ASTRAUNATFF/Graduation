using Kindergarten_school.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kindergarten_school.Controllers
{
    public class FeeController : Controller
    {
        private readonly IMongoCollection<Fee> _feeCollection;
        private readonly IMongoCollection<Attendance> _attendanceCollection;
        private readonly IMongoCollection<TransactionModel> _transactionsCollection;
        private readonly IMongoCollection<StudentModel> _studentCollection;

        public FeeController(IMongoDatabase database)
        {
            _feeCollection = database.GetCollection<Fee>("fees");
            _attendanceCollection = database.GetCollection<Attendance>("attendances");
            _transactionsCollection = database.GetCollection<TransactionModel>("transactions");
            _studentCollection = database.GetCollection<StudentModel>("students");
        }

        // Hiển thị danh sách học phí
        public IActionResult Index(string? studentId)
        {
            var students = _studentCollection.Find(_ => true).ToList()
                .Select(s => new
                {
                    s.StudentID,
                    FullName = s.LastName + " " + s.FirstName
                })
                .ToList();

            var fees = !string.IsNullOrEmpty(studentId)
                ? _feeCollection.Find(f => f.StudentId == studentId).ToList()
                : _feeCollection.Find(_ => true).ToList();

            ViewBag.Students = students;
            ViewBag.SelectedStudentId = studentId;

            return View(fees);
        }



        // Tính và cập nhật học phí dựa trên số ngày đi học
        public IActionResult CalculateFee(string studentId, decimal dailyRate)
        {
            var daysAttended = _attendanceCollection.CountDocuments(a => a.StudentID == studentId && a.Status == "Có Mặt");
            var totalFee = daysAttended * dailyRate;

            var fee = _feeCollection.Find(f => f.StudentId == studentId).FirstOrDefault();
            if (fee == null)
            {
                fee = new Fee
                {
                    StudentId = studentId,
                    DaysAttended = (int)daysAttended,
                    Amount = totalFee,
                    Status = "Chưa Đóng",
                    LastUpdated = DateTime.Now
                };
                _feeCollection.InsertOne(fee);
            }
            else
            {
                fee.DaysAttended = (int)daysAttended;
                fee.Amount = totalFee;
                fee.LastUpdated = DateTime.Now;
                _feeCollection.ReplaceOne(f => f.Id == fee.Id, fee);
            }

            return RedirectToAction("Index");
        }

        // Xác nhận thanh toán học phí và lưu vào lịch sử giao dịch
        public async Task<IActionResult> ConfirmPayment(string studentId)
        {
            var fee = _feeCollection.Find(f => f.StudentId == studentId).FirstOrDefault();
            if (fee == null)
            {
                return NotFound(new { message = "Không tìm thấy học phí cần thanh toán!" });
            }

            fee.Status = "Đã Thanh Toán";
            fee.LastUpdated = DateTime.Now;
            _feeCollection.ReplaceOne(f => f.Id == fee.Id, fee);

            var transaction = new TransactionModel
            {
                studentID = studentId,
                amount = Convert.ToDecimal(fee.Amount),
                typeoftrans = "Học phí",
                transactiondate = DateTime.Now
            };

            try
            {
                await _transactionsCollection.InsertOneAsync(transaction);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi lưu giao dịch: " + ex.Message });
            }

            return RedirectToAction("Index");
        }

        // Lấy danh sách học phí theo ID học sinh
        public IActionResult GetStudentFees(string studentId)
        {
            var studentFees = _feeCollection.Find(f => f.StudentId == studentId).ToList();
            return View("Index", studentFees);
        }

        // ================== CHỨC NĂNG THÊM - SỬA - XÓA ==================

        // Hiển thị form thêm học phí
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý thêm học phí mới
        [HttpPost]
        public IActionResult Create(Fee fee)
        {
            if (ModelState.IsValid)
            {
                fee.LastUpdated = DateTime.Now;
                _feeCollection.InsertOne(fee);
                return RedirectToAction("Index");
            }
            return View(fee);
        }

        // Hiển thị form chỉnh sửa học phí
        public IActionResult Edit(string id)
        {
            var fee = _feeCollection.Find(f => f.Id == id).FirstOrDefault();
            if (fee == null)
            {
                return NotFound();
            }
            return View(fee);
        }

        // Xử lý cập nhật học phí
        [HttpPost]
        public IActionResult Edit(Fee fee)
        {
            if (ModelState.IsValid)
            {
                fee.LastUpdated = DateTime.Now;
                _feeCollection.ReplaceOne(f => f.Id == fee.Id, fee);
                return RedirectToAction("Index");
            }
            return View(fee);
        }

        // Hiển thị xác nhận xóa
        public IActionResult Delete(string id)
        {
            var fee = _feeCollection.Find(f => f.Id == id).FirstOrDefault();
            if (fee == null)
            {
                return NotFound();
            }
            return View(fee);
        }

        // Xóa học phí
        [HttpPost, ActionName("DeleteConfirmed")]
        public IActionResult DeleteConfirmed(string id)
        {
            _feeCollection.DeleteOne(f => f.Id == id);
            return RedirectToAction("Index");
        }
    }
}
