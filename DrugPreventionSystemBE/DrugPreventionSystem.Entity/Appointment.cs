using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // Người đặt (Customer)    
        public Guid? ConsultantId { get; set; } // Tư vấn viên (lưu ID trong bảng User)
        public DateTime? AppointmentTime { get; set; } // Có thể null nếu chưa đặt thời gian
        public string Note { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }


        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        //navigation
        public User User { get; set; }
        public Consultants Consultant { get; set; }
    }

}
