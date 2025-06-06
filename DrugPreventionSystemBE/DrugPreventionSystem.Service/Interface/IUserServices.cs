using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Services
{
<<<<<<< HEAD
    public interface IUserServices
=======
    public interface IUserService
>>>>>>> e6dd2cfe35d7e42c5aaf1e3cd2417354fab25673
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task<bool> UpdateUserAsync(Guid id, User updatedUser);
        Task<bool> DeleteUserAsync(Guid id);
        bool UserExists(Guid id);
    }
}
