﻿using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.UserSearchModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task<IEnumerable<User>> SearchUsersAsync(UserSearchModel search);
        Task<IActionResult> UpdateUserProfileAsync(UserProfileUpdateRequest request);
        Task<bool> DeleteUserAsync(Guid id);
        bool UserExists(Guid id);
    }
}
