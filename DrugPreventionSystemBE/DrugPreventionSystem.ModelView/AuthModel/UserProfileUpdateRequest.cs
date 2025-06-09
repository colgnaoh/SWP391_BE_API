using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel
{
    public class UserProfileUpdateRequest
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public String? Gender { get; set; } 

        public DateTime? Dob { get; set; } 

        public string? ProfilePicUrl { get; set; }

    }
}
