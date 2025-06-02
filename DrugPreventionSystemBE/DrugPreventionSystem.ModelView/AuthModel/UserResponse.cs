using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.Text.Json.Serialization;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel
{
    public class UserResponse
    {
        public Guid Id { get; set; }          
        public string? Email { get; set; }
        public string? FirstName { get; set; }  
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Role Role { get; set; }
        public string? Address { get; set; }       
        public string? Gender { get; set; }
        public DateTime Dob { get; set; }
        public string? AgeGroup { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }    
    }
}
