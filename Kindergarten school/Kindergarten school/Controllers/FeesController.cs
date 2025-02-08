using Kindergarten_school.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

namespace Kindergarten_school.Controllers
{
    public class FeeController : Controller
    {
        private readonly IMongoCollection<Fee> _feeCollection;
        private readonly IMongoCollection<Attendance> _attendanceCollection;
        private readonly IMongoCollection<TransactionModel> _transactionsCollection;

        public FeeController(IMongoDatabase database)
        {
            _feeCollection = database.GetCollection<Fee>("fees");
            _attendanceCollection = database.GetCollection<Attendance>("attendances");
            _transactionsCollection = database.GetCollection<TransactionModel>("transactions");
        }

        // Hiển thị danh sách học phí
        public IActionResult Index()
        {
            var fees = _feeCollection.Find(_ => true).ToList();
            return View(fees);
        }

        // Tính và cập nhật học phí dựa trên số ngày đi học
        public IActionResult CalculateFee(int studentId, decimal dailyRate)
        {
            // Lấy số ngày đi học của học sinh
            var daysAttended = _attendanceCollection.CountDocuments(a => a.StudentID == studentId && a.Status == "Có Mặt");

            // Tính học phí
            var totalFee = daysAttended * dailyRate;

            // Cập nhật hoặc thêm mới Fee
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
        public async Task<IActionResult> ConfirmPayment(int studentId)
        {
            var fee = _feeCollection.Find(f => f.StudentId == studentId).FirstOrDefault();
            if (fee == null)
            {
                return NotFound(new { message = "Không tìm thấy học phí cần thanh toán!" });
            }

            // Cập nhật trạng thái học phí
            fee.Status = "Đã Thanh Toán";
            fee.LastUpdated = DateTime.Now;
            _feeCollection.ReplaceOne(f => f.Id == fee.Id, fee);

            // Lưu giao dịch vào bảng transactions
            var transaction = new TransactionModel
            {
                studentID = studentId,
                amount = Convert.ToDecimal(fee.Amount),  // Đảm bảo kiểu decimal
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
        public IActionResult GetStudentFees(int studentId)
        {
            var studentFees = _feeCollection.Find(f => f.StudentId == studentId).ToList();
            return View("Index", studentFees);
        }
    }
}
