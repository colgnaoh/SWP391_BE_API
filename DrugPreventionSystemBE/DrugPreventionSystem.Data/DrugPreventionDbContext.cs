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

            // Khai báo bảng Users
            public DbSet<User> Users { get; set; }
            public DbSet<CommunityProgram> Programs { get; set; }
            public DbSet<Course> Courses { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<Blog> Blogs { get; set; }




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

            // Global filter: exclude soft-deleted users
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        }


    }


}
