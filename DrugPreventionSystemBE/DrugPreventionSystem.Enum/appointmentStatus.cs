namespace DrugPreventionSystemBE.DrugPreventionSystem.Enum
{
    public enum AppointmentStatus
    {
        Pending,    // Mới tạo
        Confirmed,  // Đã xác nhận (có tư vấn viên & thời gian)
        Assigned,   // Đã assgin tư vấn viên
        Processing, // Đang trong quá trình tư vấn
        Completed,   // Đã hoàn thành tư vấn
        Canceled    // Đã hủy buổi hẹn
    }

}
