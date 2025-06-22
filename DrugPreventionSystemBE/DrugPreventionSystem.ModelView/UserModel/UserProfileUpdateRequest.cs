namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.UserModel
{
    public class UserProfileUpdateRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public DateTime? Dob { get; set; }
        public string? ProfilePicUrl { get; set; }
    }
}
