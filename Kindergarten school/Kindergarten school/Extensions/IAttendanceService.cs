using Kindergarten_school.Models;

namespace Kindergarten_school.Extensions
{
    public interface IAttendanceService
    {
        Task<List<Attendance>> GetAttendanceByDateRangeAsync(DateTime startDate, DateTime endDate);


    }
}
