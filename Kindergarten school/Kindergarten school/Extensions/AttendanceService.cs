using Kindergarten_school.Models;
using MongoDB.Driver;

namespace Kindergarten_school.Extensions
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IMongoCollection<Attendance> _attendances;

        public AttendanceService(IMongoDatabase database)
        {
            _attendances = database.GetCollection<Attendance>("attendances");
        }

        public async Task<List<Attendance>> GetAttendanceByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var filter = Builders<Attendance>.Filter.And(
                Builders<Attendance>.Filter.Gte(a => a.Date, startDate),
                Builders<Attendance>.Filter.Lte(a => a.Date, endDate)
            );

            return await _attendances.Find(filter).ToListAsync();
        }
    }


}
