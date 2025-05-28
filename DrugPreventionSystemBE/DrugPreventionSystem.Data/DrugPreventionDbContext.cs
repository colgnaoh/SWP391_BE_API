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
        }

   
}
