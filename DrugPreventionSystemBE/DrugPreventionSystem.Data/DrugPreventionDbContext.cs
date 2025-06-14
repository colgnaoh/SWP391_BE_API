﻿using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using Microsoft.EntityFrameworkCore;
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
            modelBuilder.Entity<Transaction>().Property(t => t.Status).HasConversion<string>();
            modelBuilder.Entity<Transaction>().Property(t => t.ServiceType).HasConversion<string>();

            //----------------------------------------------------------------------- cho đỡ rối ở trên là enh cấu hình các Enum trong DbContext này
            // Cấu hình cho List<string> trong Consultants
            modelBuilder.Entity<Consultants>()
                .Property(con => con.Qualifications)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v), // Chuyển List<string> sang JSON string
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>() // Chuyển JSON string ngược lại List<string>
                );
            //----------------------------------------------------------------------- dưới đây Khôi cấu hình các quan hệ bản trong DbContext này
            // Cấu hình mối quan hệ (Relationships)

            // Blog - User
            modelBuilder.Entity<Blog>()
                .HasOne(b => b.User)
                .WithMany(u => u.Blogs)
                .HasForeignKey(b => b.UserId)
                .IsRequired(false);

            // Cart - User
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .IsRequired(false);

            // Cart - Course
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Course)
                .WithMany(co => co.Carts)
                .HasForeignKey(c => c.CourseId)
                .IsRequired(false);

            // Consultants - User 
            modelBuilder.Entity<Consultants>()
                .HasOne(con => con.User)
                .WithMany(u => u.ConsultantProfiles)
                .HasForeignKey(con => con.UserId)
                .IsRequired(false);

            // Course - User (Người tạo khóa học)
            modelBuilder.Entity<Course>()
                .HasOne(co => co.User)
                .WithMany(u => u.CreatedCourses)
                .HasForeignKey(co => co.UserId)
                .IsRequired(false);

            // Course - Category
            modelBuilder.Entity<Course>()
                .HasOne(co => co.Category)
                .WithMany(ca => ca.Courses)
                .HasForeignKey(co => co.CategoryId)
                .IsRequired(false);

            // Lesson - Session
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Session)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.SessionId)
                .IsRequired(false);

            // Lesson - Course
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Course)
                .WithMany(co => co.Lessons)
                .HasForeignKey(l => l.CourseId)
                .IsRequired(false);

            // Lesson - User (Người tạo bài học)
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.User)
                .WithMany(u => u.CreatedLessons)
                .HasForeignKey(l => l.UserId)
                .IsRequired(false);

            // Order - Cart
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Cart)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CartId)
                .IsRequired(false);

            // OrderDetail - Order
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .IsRequired(false);

            // OrderDetail - Transaction
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Transaction)
                .WithMany(t => t.OrderDetails) 
                .HasForeignKey(od => od.TransactionId)
                .IsRequired(false);

            // Payment - User
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .IsRequired(false);

            // Question - Survey
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Survey)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.SurveyId)
                .IsRequired(false);

            // AnswerOption - Question
            modelBuilder.Entity<AnswerOption>()
                .HasOne(ao => ao.Question)
                .WithMany(q => q.AnswerOptions)
                .HasForeignKey(ao => ao.QuestionId)
                .IsRequired(false);

            // Review - Course
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Course)
                .WithMany(co => co.Reviews)
                .HasForeignKey(r => r.CourseId)
                .IsRequired(false);

            // Review - User
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .IsRequired(false);

            // Session - Course 
            modelBuilder.Entity<Session>()
                .HasOne(s => s.Course)
                .WithMany(co => co.Sessions)
                .HasForeignKey(s => s.CourseId)
                .IsRequired(); // Session BẮT BUỘC phải có Course

            // Session - User (Người tạo Session)
            modelBuilder.Entity<Session>()
                .HasOne(s => s.User)
                .WithMany(u => u.CreatedSessions)
                .HasForeignKey(s => s.UserId)
                .IsRequired(false);

            // SurveyResult - User
            modelBuilder.Entity<SurveyResult>()
                .HasOne(sr => sr.User)
                .WithMany(u => u.SurveyResults)
                .HasForeignKey(sr => sr.UserId)
                .IsRequired(false);

            // SurveyResult - Survey
            modelBuilder.Entity<SurveyResult>()
                .HasOne(sr => sr.Survey)
                .WithMany(s => s.SurveyResults)
                .HasForeignKey(sr => sr.SurveyId)
                .IsRequired(false);

            // SurveyResult - CommunityProgram 
            modelBuilder.Entity<SurveyResult>()
                .HasOne(sr => sr.Program)
                .WithMany(cp => cp.SurveyResults)
                .HasForeignKey(sr => sr.ProgramId) 
                .IsRequired(false);

            // UserAnswerLog - SurveyResult
            modelBuilder.Entity<UserAnswerLog>()
                .HasOne(ual => ual.SurveyResult)
                .WithMany(sr => sr.UserAnswerLogs)
                .HasForeignKey(ual => ual.SurveyResultId)
                .IsRequired(false);

            // UserAnswerLog - Question
            modelBuilder.Entity<UserAnswerLog>()
                .HasOne(ual => ual.Question)
                .WithMany(q => q.UserAnswerLogs)
                .HasForeignKey(ual => ual.QuestionId)
                .IsRequired(false);

            // UserAnswerLog - AnswerOption
            modelBuilder.Entity<UserAnswerLog>()
                .HasOne(ual => ual.AnswerOption)
                .WithMany(ao => ao.UserAnswerLogs) 
                .HasForeignKey(ual => ual.AnswerOptionId)
                .IsRequired(false);

            // UserAnswerLog - CommunityProgram 
            modelBuilder.Entity<UserAnswerLog>()
                .HasOne(ual => ual.Program)
                .WithMany(cp => cp.UserAnswerLogs) 
                .HasForeignKey(ual => ual.ProgramId) 
                .IsRequired(false);

            // Transaction - Consultants
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Consultant)
                .WithMany(con => con.Transactions) 
                .HasForeignKey(t => t.ConsultantId)
                .IsRequired(false);

            // Transaction - Course
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Course)
                .WithMany(co => co.Transactions) 
                .HasForeignKey(t => t.CourseId)
                .IsRequired(false);
            modelBuilder.Entity<Transaction>()
             .HasOne(t => t.Program) 
             .WithMany(cp => cp.Transactions) 
             .HasForeignKey(t => t.ProgramId) 
             .IsRequired(false);
            // Global filter: exclude soft-deleted users
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        }


    }


}
