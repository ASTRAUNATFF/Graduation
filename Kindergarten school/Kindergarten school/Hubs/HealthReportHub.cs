using Kindergarten_school.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Security.Claims;

namespace Kindergarten_school.Hubs
{
    public class HealthReportHub : Hub
    {
        private readonly IMongoCollection<Parent> _parentCollection;
        private readonly IMongoCollection<StudentModel> _studentCollection;

        public HealthReportHub(IMongoDatabase database)
        {
            _parentCollection = database.GetCollection<Parent>("parents");
            _studentCollection = database.GetCollection<StudentModel>("students");
        }

        public override async Task OnConnectedAsync()
        {
            // Lấy UserID từ Claims 
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // Nếu không có UserID, sử dụng ConnectionId thay thế
                userId = Context.ConnectionId;
            }

            // Lấy danh sách học sinh theo ParentID
            var listStudents = await _studentCollection.Find(s => s.ParentID == userId).ToListAsync();

            foreach (var student in listStudents)
            {
                // Thêm Connection vào nhóm theo StudentID
                await Groups.AddToGroupAsync(Context.ConnectionId, $"student_{student.StudentID}");


            }
            // Lưu userId vào Context.Items để sử dụng khi disconnect
            Context.Items["UserId"] = userId;

            // Thêm vào nhóm
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"User hủy: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}