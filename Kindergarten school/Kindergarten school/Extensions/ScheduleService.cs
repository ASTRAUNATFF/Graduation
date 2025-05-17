using Kindergarten_school.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kindergarten_school.Services;

public class ScheduleService : IScheduleService
{
    private readonly IMongoCollection<ScheduleModel> _scheduleCollection;
    private readonly IMongoCollection<StudentModel> _studentCollection;
    private readonly IMongoCollection<TeacherModel> _teacherCollection;
    private readonly IMongoCollection<ClassModel> _classCollection;

    public ScheduleService(IMongoClient client)
    {     
        var database = client.GetDatabase("db2024"); // Đảm bảo tên database chính xác
        _scheduleCollection = database.GetCollection<ScheduleModel>("schedules"); // Đảm bảo tên collection chính xác
        _studentCollection = database.GetCollection<StudentModel>("students");
        _teacherCollection = database.GetCollection<TeacherModel>("teachers");
        _classCollection = database.GetCollection<ClassModel>("classes");
    }

    // Lấy tất cả lịch học
    public async Task<List<ScheduleModel>> GetAllSchedulesAsync()
    {
        return await _scheduleCollection.Find(_ => true).ToListAsync();
    }

    // Lấy lịch học theo ID
    public async Task<ScheduleModel> GetScheduleByIdAsync(string id)
    {
        return await _scheduleCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    // Thêm lịch học mới
    public async Task CreateScheduleAsync(ScheduleModel schedule)
    {
        await _scheduleCollection.InsertOneAsync(schedule);
    }

    // Cập nhật lịch học
    public async Task UpdateScheduleAsync(string id, ScheduleModel schedule)
    {
        await _scheduleCollection.ReplaceOneAsync(s => s.Id == id, schedule);
    }

    // Xóa lịch học
    public async Task DeleteScheduleAsync(string id)
    {
        await _scheduleCollection.DeleteOneAsync(s => s.Id == id);
    }

    public Task<IEnumerable<string>> GetSubjectsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<ScheduleModel>> GetSchedulesByClassAsync(string classId)
    {
        return await _scheduleCollection.Find(s => s.ClassId == classId).ToListAsync();
    }

    

    public async Task<string?> GetClassIdByStudentIdAsync(string studentId)
    {
        var student = await _studentCollection
            .Find(s => s.StudentID == studentId)
            .FirstOrDefaultAsync();

        return student?.ClassID;
    }

    public async Task<string?> GetStudentIdByParentIdAsync(string parentId)
    {
        var student = await _studentCollection
            .Find(s => s.ParentID == parentId)
            .FirstOrDefaultAsync();

        return student?.StudentID;
    }

    public async Task<string?> GetClassIdByTeacherIdAsync(string teacherId)
    {
        var teacher = await _teacherCollection
            .Find(t => t.TeacherID == teacherId)
            .FirstOrDefaultAsync();

        return teacher?.ClassID;
    }

    public async Task<List<ScheduleModel>> GetSchedulesByClassIdAsync(string classId)
    {
        return await _scheduleCollection.Find(s => s.ClassId == classId).ToListAsync();
    }

    public async Task<List<ClassModel>> GetAllClassesAsync()
    {
        return await _classCollection.Find(_ => true).ToListAsync();
    }

}
