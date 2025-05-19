using System.Threading.Tasks;
using Kindergarten_school.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace Kindergarten_school.Extensions
{
    public class StudentService : IStudentService
    {
        private readonly IMongoCollection<StudentModel> _students;

        public StudentService(IMongoClient client)
        {
            var database = client.GetDatabase("db2024");
            _students = database.GetCollection<StudentModel>("students");
        }

        public async Task<List<StudentModel>> GetAllStudentsAsync()
        {
            return await _students.Find(student => true).ToListAsync();
        }

        public async Task<StudentModel> GetStudentByIdAsync(int id)
        {
            return await _students.Find(student => student.StudentID == id.ToString()).FirstOrDefaultAsync();

        }

        public async Task<StudentModel> GetByIdAsync(string id)
        {
            return await _students.Find(student => student.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(StudentModel student)
        {
            await _students.InsertOneAsync(student);
        }

        public async Task UpdateAsync(string id, StudentModel student)
        {
            await _students.ReplaceOneAsync(s => s.Id == id, student);
        }

        public async Task DeleteAsync(string id)
        {
            await _students.DeleteOneAsync(student => student.Id == id);
        }

        public async Task<List<StudentModel>> GetAllAsync()
        {
            return await _students.Find(_ => true).ToListAsync();
        }
    }
}
