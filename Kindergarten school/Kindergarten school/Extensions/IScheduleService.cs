using System.Collections.Generic;
using System.Threading.Tasks;
using Kindergarten_school.Models;
using Kindergarten_school.Services;

namespace Kindergarten_school.Services
{
    public interface IScheduleService
    {
        /// <summary>
        /// Lấy danh sách tất cả các lịch học.
        /// </summary>
        Task<List<ScheduleModel>> GetAllSchedulesAsync();

        /// <summary>
        /// Lấy thông tin lịch học theo ID.
        /// </summary>
        Task<ScheduleModel> GetScheduleByIdAsync(string id);

        /// <summary>
        /// Thêm mới một lịch học.
        /// </summary>
        Task CreateScheduleAsync(ScheduleModel schedule);

        /// <summary>
        /// Cập nhật lịch học theo ID.
        /// </summary>
        Task UpdateScheduleAsync(string id, ScheduleModel schedule);

        /// <summary>
        /// Xóa lịch học theo ID.
        /// </summary>
        Task DeleteScheduleAsync(string id);

        /// <summary>
        /// Lấy danh sách môn học.
        /// </summary>
        Task<IEnumerable<string>> GetSubjectsAsync();

        Task<List<ScheduleModel>> GetSchedulesByClassAsync(string classId);

        Task<List<ScheduleModel>> GetSchedulesByClassIdAsync(string classId);

        Task<string?> GetClassIdByStudentIdAsync(string studentId);

        Task<string?> GetStudentIdByParentIdAsync(string parentId);

        Task<string?> GetClassIdByTeacherIdAsync(string teacherId);

        Task<List<ClassModel>> GetAllClassesAsync();
    }
}
