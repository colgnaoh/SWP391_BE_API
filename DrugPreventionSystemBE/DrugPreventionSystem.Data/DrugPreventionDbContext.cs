using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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

            // Cấu hình cho thuộc tính Role của User
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion(new EnumToStringConverter<Role>()); 

            modelBuilder.Entity<User>()
               .Property(u => u.AgeGroup)
               .HasConversion(new EnumToStringConverter<AgeGroup>());

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Status)
                      .HasConversion<string>();

                entity.Property(p => p.PaymentMethod)
                      .HasConversion<string>();

                entity.Property(p => p.Amount)
                      .HasColumnType("decimal(10,2)");

                entity.Property(p => p.OrganizationShare)
                      .HasColumnType("decimal(10,2)");

                entity.Property(p => p.ConsultantShare)
                      .HasColumnType("decimal(10,2)");
            });

            // Transaction configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Status)
                      .HasConversion<string>();

                entity.Property(t => t.ServiceType)
                      .HasConversion<string>();

                entity.Property(t => t.Amount)
                      .HasColumnType("decimal(10,2)");
            });

            // Global filter: exclude soft-deleted users
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        }


    }


}
