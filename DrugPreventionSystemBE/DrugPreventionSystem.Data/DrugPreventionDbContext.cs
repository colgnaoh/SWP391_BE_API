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
            public DbSet<CommunityProgram> Programs { get; set; }
            public DbSet<Course> Courses { get; set; }
            public DbSet<Category> Categories { get; set; }

    }


}
