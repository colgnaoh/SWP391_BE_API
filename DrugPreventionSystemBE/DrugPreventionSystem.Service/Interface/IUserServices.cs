using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.Enum;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.ResponseModel;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.UserSearchModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Services
{
    public interface IUserService
    {
        Task<IActionResult> CreateUserAsync(UserRegisterRequest request);
        Task<GetUserByPageResponseModel> GetUsersByPageAsync(int pageNumber, int pageSize, Role? role = null, string? searchCondition = null, bool? isVerified = null);
        Task<SingleUserResponseModel> GetUserByIdAsync(Guid id);
        Task<SearchUserResponseModel> SearchUsersAsync(UserSearchModel search);
        Task<IActionResult> UpdateUserProfileAsync(UserProfileUpdateRequest request);
        Task<bool> DeleteUserAsync(Guid id);
        bool UserExists(Guid id);
        Task<IActionResult> ChangePasswordAsync(string currentPassword, string newPassword);
    }
}
