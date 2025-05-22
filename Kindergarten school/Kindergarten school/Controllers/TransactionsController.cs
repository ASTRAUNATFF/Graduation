using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Kindergarten_school.Models;
using System;
using System.Threading.Tasks;

namespace Kindergarten_school.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly IMongoCollection<TransactionModel> _transactionCollection;

        public TransactionsController(IMongoDatabase database)
        {
            _transactionCollection = database.GetCollection<TransactionModel>("transactions");
        }

        // Hàm lưu giao dịch
        public async Task<IActionResult> SaveTransaction(string studentId, decimal amount, string type)
        {
            var transaction = new TransactionModel
            {
                studentID = studentId,
                amount = amount,
                typeoftrans = type,
                transactiondate = DateTime.UtcNow
            };

            await _transactionCollection.InsertOneAsync(transaction);
            return Ok(new { message = "Giao dịch đã được lưu thành công!" });
        }

        // Hiển thị danh sách giao dịch
        public IActionResult Index()
        {
            var transactions = _transactionCollection.Find(_ => true).ToList();
            return View(transactions);
        }
    }
}
