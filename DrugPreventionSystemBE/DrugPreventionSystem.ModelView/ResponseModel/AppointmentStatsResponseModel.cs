namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class AppointmentStatsResponseModel
    {
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int AssignedAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public Dictionary<string, int> AppointmentsByConsultant { get; set; }
    }
}
