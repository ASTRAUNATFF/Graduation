using Kindergarten_school.Extensions;
using Kindergarten_school.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kindergarten_school.Extensions
{
    public class ScheduleService : IScheduleService
    {
        private readonly IMongoCollection<ScheduleModel> _scheduleCollection;

        public ScheduleService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("db2024"); // Tên database của bạn
            _scheduleCollection = database.GetCollection<ScheduleModel>("schedules"); // Tên collection
        }

        public async Task<List<ScheduleModel>> GetAllSchedulesAsync()
        {
            return await _scheduleCollection.Find(_ => true).ToListAsync();
        }

        public async Task<ScheduleModel> GetScheduleByIdAsync(string id)
        {
            return await _scheduleCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateScheduleAsync(ScheduleModel schedule)
        {
            await _scheduleCollection.InsertOneAsync(schedule);
        }

        public async Task UpdateScheduleAsync(string id, ScheduleModel schedule)
        {
            await _scheduleCollection.ReplaceOneAsync(x => x.Id == id, schedule);
        }

        public async Task DeleteScheduleAsync(string id)
        {
            await _scheduleCollection.DeleteOneAsync(x => x.Id == id);
        }

        public Task<IEnumerable<object>> GetSubjectsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
