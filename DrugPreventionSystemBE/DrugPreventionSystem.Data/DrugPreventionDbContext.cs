using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using Microsoft.EntityFrameworkCore;

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

        // Nếu có các bảng khác thì khai báo ở đây
        // public DbSet<OtherEntity> Others { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnType("bigint")
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.VerificationToken)
                    .HasColumnType("nvarchar(max)");
                entity.Property(e => e.Email)
                    .HasColumnType("nvarchar(255)");
                entity.Property(e => e.IsVerified)
                    .HasColumnType("bit");
                // Configure other properties as needed
            });
        }
    }

   
}
