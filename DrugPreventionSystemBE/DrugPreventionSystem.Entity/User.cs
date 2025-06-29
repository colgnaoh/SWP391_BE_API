
﻿using DrugPreventionSystemBE.DrugPreventionSystem.Core;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Entity
{
    public class User : BaseEnity
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public DateTime Dob { get; set; }
        public Role Role { get; set; }
        public AgeGroup AgeGroup { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? VerificationToken { get; set; }
        public DateTime? VerificationTokenExpires { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; 
        public bool IsDeleted { get; set; } = false;
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
        public string? ProfilePicUrl { get; set; }
        public User()
        {
            IsVerified = false; // mặc định là chưa xác thực
        }

        public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
        public ICollection<Lesson> CreatedLessons { get; set; } = new List<Lesson>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Session> CreatedSessions { get; set; } = new List<Session>();
        public ICollection<SurveyResult> SurveyResults { get; set; } = new List<SurveyResult>();
        public ICollection<Consultants> ConsultantProfiles { get; set; } = new List<Consultants>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<OrderLog> OrderLogs { get; set; } = new List<OrderLog>();
    }
}
