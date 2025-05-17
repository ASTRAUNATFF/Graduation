using Kindergarten_school.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Security.Claims;

namespace Kindergarten_school.Hubs
{
    public class SchoolHub : Hub
    {
        private readonly IMongoCollection<Parent> _parentCollection;
        private readonly IMongoCollection<StudentModel> _studentCollection;
        private readonly IMongoCollection<ClassModel> _classCollection;

        public SchoolHub(IMongoDatabase database)
        {
            _parentCollection = database.GetCollection<Parent>("parents");
            _studentCollection = database.GetCollection<StudentModel>("students");
            _classCollection = database.GetCollection<ClassModel>("classes");
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

            var parent = await _parentCollection.Find(s => s.AccountID == userId).FirstOrDefaultAsync();

            // Lấy danh sách học sinh theo ParentID
            var listStudents = await _studentCollection.Find(s => s.ParentID == parent.ParentID).ToListAsync();

            foreach (var student in listStudents)
            {
                // Thêm Connection vào nhóm theo StudentID
                await Groups.AddToGroupAsync(Context.ConnectionId, $"student_{student.StudentID}");

                // Lấy ClassID của học sinh và thêm vào nhóm theo ClassID
                if (!string.IsNullOrEmpty(student.ClassID))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"class_{student.ClassID}");
                }
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