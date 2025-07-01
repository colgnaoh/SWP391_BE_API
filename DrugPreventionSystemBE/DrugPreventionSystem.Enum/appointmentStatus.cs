namespace DrugPreventionSystemBE.DrugPreventionSystem.Enum
{
    public enum AppointmentStatus
    {
        Pending,    // Mới tạo
        Confirmed,  // Đã xác nhận (có tư vấn viên & thời gian)
        Assigned,   // Đã assgin tư vấn viên
        Completed   // Đã hoàn thành tư vấn
    }

}
