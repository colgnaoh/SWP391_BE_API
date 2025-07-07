using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Review : BaseEnity
    {
        public Guid? CourseId { get; set; }
        public Course? Course { get; set; }
        public Guid? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }
        public Guid? UserId { get; set; }
        public User? User { get; set; } // Assuming a User model exists
        public int? Rating { get; set; }
        public string? Comment { get; set; }

    }
}
