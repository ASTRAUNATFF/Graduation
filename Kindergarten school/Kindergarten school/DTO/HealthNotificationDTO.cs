namespace Kindergarten_school.DTO
{
    public class HealthNotificationDTO
    {

        public string? Id { get; set; }
        public int studentId { get; set; }

        public DateTime createDate { get; set; }

        public string? HealthStatus { get; set; }
        public string? StudentName { get; set; }
    }
}