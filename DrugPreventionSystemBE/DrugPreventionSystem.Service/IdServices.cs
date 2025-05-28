using DrugPreventionSystemBE.DrugPreventionSystem.Data; // namespace chứa DbContext
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class IdServices
    {
        private readonly DrugPreventionDbContext _context;

        public IdServices(DrugPreventionDbContext context)
        {
            _context = context;
        }

        public async Task<long> GenerateNextUserIdAsync()
        {
            if (!await _context.Users.AnyAsync())
                return 1;

            var maxId = await _context.Users
                .Select(u => (long)u.Id)  // ép kiểu rõ ràng ở đây
                .MaxAsync();

            return maxId + 1;
        }

    }
}
