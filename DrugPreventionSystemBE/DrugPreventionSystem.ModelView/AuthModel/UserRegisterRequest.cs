using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using System.ComponentModel.DataAnnotations;

namespace DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel
{
    public class UserRegisterRequest
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string PhoneNumber { get; set; }     

        [Required]
        public string Role { get; set; }
        public string Address { get; set; }

        public string Gender { get; set; }

        [Required]
        public DateTime Dob { get; set; }
        public string ProfilePicUrl { get; set; }
    }
}
