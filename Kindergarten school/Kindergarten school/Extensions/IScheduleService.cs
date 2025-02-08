using System.Collections.Generic;
using System.Threading.Tasks;
using Kindergarten_school.Models;

namespace Kindergarten_school.Extensions
{
    public interface IScheduleService
    {
        Task<List<ScheduleModel>> GetAllSchedulesAsync();
        Task<ScheduleModel> GetScheduleByIdAsync(string id);
        Task CreateScheduleAsync(ScheduleModel schedule);
        Task UpdateScheduleAsync(string id, ScheduleModel schedule);
        Task DeleteScheduleAsync(string id);
        Task<IEnumerable<object>> GetSubjectsAsync();
    }
}
