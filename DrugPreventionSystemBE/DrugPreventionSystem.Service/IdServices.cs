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

        public Guid GenerateNextUserId()
        {
            return Guid.NewGuid();
        }

    

    }
}
