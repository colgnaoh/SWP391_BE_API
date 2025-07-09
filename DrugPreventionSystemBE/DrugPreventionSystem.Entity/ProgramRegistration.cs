namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class ProgramRegistration
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid ProgramId { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;

        public CommunityProgram Program { get; set; } = default!;
        public User User { get; set; } = default!;
    }

}
