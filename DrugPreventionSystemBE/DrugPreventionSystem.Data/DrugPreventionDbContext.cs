//using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System;


namespace DrugPreventionSystemBE.DrugPreventionSystem.Data
{
    public class DrugPreventionDbContext : DbContext
    {

        // Constructor nhận options, dùng để config chuỗi kết nối
        public DrugPreventionDbContext(DbContextOptions<DrugPreventionDbContext> options)
            : base(options)
        {
        }

        // Khai báo Entities
        public DbSet<User> Users { get; set; }
        public DbSet<CommunityProgram> Programs { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Consultants> consultants { get; set; }
        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Session> Sessions { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<Survey> Surveys { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<SurveyResult> SurveyResults { get; set; }
        public DbSet<UserAnswerLog> UserAnswerLogs { get; set; }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<OrderLog> OrderLogs { get; set; }

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<ProgramRegistration> ProgramRegistrations { get; set; }

        public DbSet<ProgramFavorite> ProgramFavorites { get; set; }
        public DbSet<Favorite> Favorites { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Thêm các converters cho tất cả các Enum lưu thành dưới dạng chuỗi
            modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<string>();
            modelBuilder.Entity<User>().Property(u => u.AgeGroup).HasConversion<string>();
            modelBuilder.Entity<Cart>().Property(c => c.Status).HasConversion<string>();
            modelBuilder.Entity<CommunityProgram>().Property(cp => cp.Type).HasConversion<string>();
            modelBuilder.Entity<Consultants>().Property(con => con.Status).HasConversion<string>();
            modelBuilder.Entity<Course>().Property(co => co.Status).HasConversion<string>();
            modelBuilder.Entity<Course>().Property(co => co.TargetAudience).HasConversion<string>();
            modelBuilder.Entity<Lesson>().Property(l => l.LessonType).HasConversion<string>();
            modelBuilder.Entity<Order>().Property(o => o.Status).HasConversion<string>();
            modelBuilder.Entity<OrderDetail>().Property(od => od.ServiceType).HasConversion<string>();
            modelBuilder.Entity<Question>().Property(q => q.QuestionType).HasConversion<string>();
            modelBuilder.Entity<Survey>().Property(s => s.Type).HasConversion<string>();
            modelBuilder.Entity<SurveyResult>().Property(sr => sr.RiskLevel).HasConversion<string>();
            modelBuilder.Entity<Payment>().Property(p => p.Status).HasConversion<string>();
            modelBuilder.Entity<Payment>().Property(p => p.PaymentMethod).HasConversion<string>();
            modelBuilder.Entity<Payment>().Property(p => p.PaymentMethod).HasConversion<string>();
            modelBuilder.Entity<Transaction>().Property(t => t.Status).HasConversion<string>();
            modelBuilder.Entity<Transaction>().Property(t => t.ServiceType).HasConversion<string>();
            modelBuilder.Entity<Cart>().Property(c => c.Discount).HasPrecision(10, 2);
            modelBuilder.Entity<Cart>().Property(c => c.Price).HasPrecision(10, 2);
            modelBuilder.Entity<Consultants>().Property(con => con.Salary).HasPrecision(10, 2);
            modelBuilder.Entity<Course>().Property(co => co.Discount).HasPrecision(10, 2);
            modelBuilder.Entity<Course>().Property(co => co.Price).HasPrecision(10, 2);
            modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(10, 2);
            modelBuilder.Entity<OrderDetail>().Property(od => od.Amount).HasPrecision(10, 2);
            modelBuilder.Entity<Appointment>().Property(ad => ad.Status).HasConversion<string>();
            modelBuilder.Entity<Course>().Property(c => c.RiskLevel).HasConversion<string>();
            modelBuilder.Entity<CommunityProgram>().Property(p => p.RiskLevel).HasConversion<string>();
            modelBuilder.Entity<Question>().Property(q => q.QuestionType).HasConversion<string>();
            modelBuilder.Entity<Survey>().Property(s => s.Type).HasConversion<string>();
            modelBuilder.Entity<SurveyResult>().Property(sr => sr.RiskLevel).HasConversion<string>();
            modelBuilder.Entity<Favorite>().Property(f => f.TargetType).HasConversion<string>();



            //----------------------------------------------------------------------- cho đỡ rối ở trên là enh cấu hình các Enum trong DbContext này
            // Cấu hình cho List<string> trong Consultants
            //modelBuilder.Entity<Consultants>()
            //    .Property(con => con.Qualifications)
            //    .HasConversion(
            //        v => JsonConvert.SerializeObject(v),
            //        v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>()
            //    )
            //    .Metadata.SetValueComparer(new ValueComparer<ICollection<string>>(
            //        (c1, c2) => c1.OrderBy(s => s).SequenceEqual(c2.OrderBy(s => s)),
            //        c => c.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode()))
            //    ));
            //----------------------------------------------------------------------- dưới đây Khôi cấu hình các quan hệ bản trong DbContext này
            // Cấu hình mối quan hệ (Relationships)

            // Blog - User
            modelBuilder.Entity<Blog>().HasOne(b => b.User).WithMany(u => u.Blogs).HasForeignKey(b => b.UserId).IsRequired(false);

            // Cart - User
            modelBuilder.Entity<Cart>().HasOne(c => c.User).WithMany(u => u.Carts).HasForeignKey(c => c.UserId).IsRequired(false);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Order)          // Một CartItem thuộc về MỘT Order
                .WithMany(o => o.Carts)       // Một Order có NHIỀU CartItem
                .HasForeignKey(c => c.OrderId)  // Khóa ngoại là OrderId trong Cart
                .IsRequired(false);           // OrderId là nullable

            // Cart - Course
            modelBuilder.Entity<Cart>().HasOne(c => c.Course).WithMany(co => co.Carts).HasForeignKey(c => c.CourseId).IsRequired(false);

            // Consultants - User
            modelBuilder.Entity<Consultants>().HasOne(con => con.User).WithMany(u => u.ConsultantProfiles).HasForeignKey(con => con.UserId).IsRequired(false);

            // Course - User (Người tạo khóa học)
            modelBuilder.Entity<Course>().HasOne(co => co.User).WithMany(u => u.CreatedCourses).HasForeignKey(co => co.UserId).IsRequired(false);

            // Course - Category
            modelBuilder.Entity<Course>().HasOne(co => co.Category).WithMany(ca => ca.Courses).HasForeignKey(co => co.CategoryId).IsRequired(false);

            // Lesson - Session
            modelBuilder.Entity<Lesson>().HasOne(l => l.Session).WithMany(s => s.Lessons).HasForeignKey(l => l.SessionId).IsRequired(false);

            // Lesson - Course
            modelBuilder.Entity<Lesson>().HasOne(l => l.Course).WithMany(co => co.Lessons).HasForeignKey(l => l.CourseId).IsRequired(false);

            // Lesson - User (Người tạo bài học)
            modelBuilder.Entity<Lesson>().HasOne(l => l.User).WithMany(u => u.CreatedLessons).HasForeignKey(l => l.UserId).IsRequired(false);

            // ĐÃ XÓA dòng này:
            // modelBuilder.Entity<Order>().HasOne(o => o.Cart).WithMany(c => c.Orders).HasForeignKey(o => o.CartId).IsRequired(false);

            // Order - User (Bổ sung mối quan hệ 1-n)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .IsRequired(); // Order BẮT BUỘC phải có User

            // OrderDetail - Order
            modelBuilder.Entity<OrderDetail>().HasOne(od => od.Order).WithMany(o => o.OrderDetails).HasForeignKey(od => od.OrderId).IsRequired(false);

            // OrderDetail - Transaction
            modelBuilder.Entity<OrderDetail>().HasOne(od => od.Transaction).WithMany(t => t.OrderDetails).HasForeignKey(od => od.TransactionId).IsRequired(false);

            // OrderDetail - Course (Bổ sung mối quan hệ 1-n)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Course)
                .WithMany(c => c.OrderDetails)
                .HasForeignKey(od => od.CourseId)
                .IsRequired(); // OrderDetail BẮT BUỘC phải có Course

            // OrderLog - Order (Bổ sung mối quan hệ 1-n)
            modelBuilder.Entity<OrderLog>()
                .HasOne(ol => ol.Order)
                .WithMany(o => o.OrderLogs)
                .HasForeignKey(ol => ol.OrderId)
                .IsRequired(); // Log BẮT BUỘC phải có Order

            // OrderLog - User (Bổ sung mối quan hệ 1-n)
            modelBuilder.Entity<OrderLog>()
                .HasOne(ol => ol.User)
                .WithMany(u => u.OrderLogs)
                .HasForeignKey(ol => ol.UserId)
                .IsRequired(false);

            // OrderLog - Cart (Cấu hình bạn đã thêm, tôi giữ nguyên)
            modelBuilder.Entity<OrderLog>().HasOne(ol => ol.Cart).WithMany(c => c.OrderLogs).HasForeignKey(ol => ol.CartId).IsRequired(false);

            // Payment - User
            modelBuilder.Entity<Payment>().HasOne(p => p.User).WithMany(u => u.Payments).HasForeignKey(p => p.UserId).IsRequired(false);

            // Question - Survey
            modelBuilder.Entity<Question>().HasOne(q => q.Survey).WithMany(s => s.Questions).HasForeignKey(q => q.SurveyId).IsRequired(false);

            // AnswerOption - Question
            modelBuilder.Entity<AnswerOption>().HasOne(ao => ao.Question).WithMany(q => q.AnswerOptions).HasForeignKey(ao => ao.QuestionId).IsRequired(false);

            // Review - Course
            modelBuilder.Entity<Review>().HasOne(r => r.Course).WithMany(co => co.Reviews).HasForeignKey(r => r.CourseId).IsRequired(false);

            // Review - User
            modelBuilder.Entity<Review>().HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId).IsRequired(false);

            // Session - Course
            modelBuilder.Entity<Session>().HasOne(s => s.Course).WithMany(co => co.Sessions).HasForeignKey(s => s.CourseId).IsRequired(); // Session BẮT BUỘC phải có Course

            // Session - User (Người tạo Session)
            modelBuilder.Entity<Session>().HasOne(s => s.User).WithMany(u => u.CreatedSessions).HasForeignKey(s => s.UserId).IsRequired(false);

            // SurveyResult - User
            modelBuilder.Entity<SurveyResult>().HasOne(sr => sr.User).WithMany(u => u.SurveyResults).HasForeignKey(sr => sr.UserId).IsRequired(false);

            // SurveyResult - Survey
            modelBuilder.Entity<SurveyResult>().HasOne(sr => sr.Survey).WithMany(s => s.SurveyResults).HasForeignKey(sr => sr.SurveyId).IsRequired(false);

            // SurveyResult - CommunityProgram
            modelBuilder.Entity<SurveyResult>().HasOne(sr => sr.Program).WithMany(cp => cp.SurveyResults).HasForeignKey(sr => sr.ProgramId).IsRequired(false);

            // UserAnswerLog - SurveyResult
            modelBuilder.Entity<UserAnswerLog>().HasOne(ual => ual.SurveyResult).WithMany(sr => sr.UserAnswerLogs).HasForeignKey(ual => ual.SurveyResultId).IsRequired(false);

            // UserAnswerLog - Question
            modelBuilder.Entity<UserAnswerLog>().HasOne(ual => ual.Question).WithMany(q => q.UserAnswerLogs).HasForeignKey(ual => ual.QuestionId).IsRequired(false);

            // UserAnswerLog - AnswerOption
            modelBuilder.Entity<UserAnswerLog>().HasOne(ual => ual.AnswerOption).WithMany(ao => ao.UserAnswerLogs).HasForeignKey(ual => ual.AnswerOptionId).IsRequired(false);

            // UserAnswerLog - CommunityProgram
            modelBuilder.Entity<UserAnswerLog>().HasOne(ual => ual.Program).WithMany(cp => cp.UserAnswerLogs).HasForeignKey(ual => ual.ProgramId).IsRequired(false);

            // Transaction - Consultants
            modelBuilder.Entity<Transaction>().HasOne(t => t.Consultant).WithMany(con => con.Transactions).HasForeignKey(t => t.ConsultantId).IsRequired(false);

            // Transaction - Course
            modelBuilder.Entity<Transaction>().HasOne(t => t.Course).WithMany(co => co.Transactions).HasForeignKey(t => t.CourseId).IsRequired(false);

            // Transaction - Program
            modelBuilder.Entity<Transaction>().HasOne(t => t.Program).WithMany(cp => cp.Transactions).HasForeignKey(t => t.ProgramId).IsRequired(false);

            //appoiment - user
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany()  // You can replace with .WithMany(u => u.Appointments) if needed
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //appointment - consultant
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Consultant)
                .WithMany()  // Or WithMany(u => u.Consultations)
                .HasForeignKey(a => a.ConsultantId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Order)          
                .WithMany(o => o.Carts)        
                .HasForeignKey(c => c.OrderId)  
                .IsRequired(false)             
                .OnDelete(DeleteBehavior.SetNull);

            //programFavorite - user
            modelBuilder.Entity<ProgramFavorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.ProgramFavorites)
                .HasForeignKey(f => f.UserId);

            //programFavorite - CommunityProgram
            modelBuilder.Entity<ProgramFavorite>()
                .HasOne(f => f.Program)
                .WithMany(p => p.ProgramFavorites)
                .HasForeignKey(f => f.ProgramId);

            //Favorite - user
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId);

            // Global filter: exclude soft-deleted users
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Survey>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<Question>().HasQueryFilter(q => !q.IsDeleted);
            modelBuilder.Entity<AnswerOption>().HasQueryFilter(ao => !ao.IsDeleted);
            modelBuilder.Entity<Blog>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<Session>().HasQueryFilter(se => !se.IsDeleted);
            modelBuilder.Entity<Lesson>().HasQueryFilter(l => !l.IsDeleted);
        }
    }
}