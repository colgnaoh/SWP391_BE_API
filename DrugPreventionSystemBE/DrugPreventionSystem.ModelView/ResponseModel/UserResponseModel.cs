namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel
{
    public class UserResponseModel
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime Dob { get; set; }
        public string? ProfilePicUrl { get; set; }
        public string? Role { get; set; }
        public bool? IsVerified { get; set; }    
    }
}
