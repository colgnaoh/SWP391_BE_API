using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.UserSearchModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Services
{
    public class UserService : IUserService
    {
        private readonly DrugPreventionDbContext _context;

        public UserService(DrugPreventionDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                                 //.Where(u => !u.IsDeleted)
                                 .ToListAsync();
        }


        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> UpdateUserAsync(Guid id, User updatedUser)
        {
            if (id != updatedUser.Id)
                return false;

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.Address = updatedUser.Address;
            user.Gender = updatedUser.Gender;
            user.Dob = updatedUser.Dob;
            user.Role = updatedUser.Role;
            user.AgeGroup = updatedUser.AgeGroup;
            user.IsVerified = updatedUser.IsVerified;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                    return false;

                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            user.IsDeleted = true; // Soft delete
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }


        public bool UserExists(Guid id)
        {
            return _context.Users.Any(u => u.Id == id);
        }

        public Task<IEnumerable<User>> SearchUsersAsync(UserSearchModel search)
        {
            throw new NotImplementedException();
        }
    }
}
