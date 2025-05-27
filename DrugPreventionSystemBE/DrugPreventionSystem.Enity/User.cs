
﻿using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Enity
{
    public class User : BaseEnity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateTime Dob { get; set; }
        public Role Role { get; set; }
        public string AgeGroup { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationToken { get; set; }
        public DateTime? VerificationTokenExpires { get; set; }
        public User()
        {
            IsVerified = false; // mặc định là chưa xác thực
        }
    }
}
