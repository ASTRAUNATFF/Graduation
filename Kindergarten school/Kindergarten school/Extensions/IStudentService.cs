using Kindergarten_school.Models;
using System.Collections.Generic;

namespace Kindergarten_school.Extensions
{
    public interface IStudentService
    {
        
        Task<StudentModel> GetByIdAsync(string id);
        Task AddAsync(StudentModel student);
        Task UpdateAsync(string id, StudentModel updatedStudent);
        Task DeleteAsync(string id);
        Task<List<StudentModel>> GetAllAsync();
    }
}
