using DrugPreventionSystemBE.DrugPreventionSystem.Entity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface
{
    public interface IAuthenticationService
    {
        Task<IActionResult> RegisterUserAsync(UserRegisterRequest request);
        Task<IActionResult> ConfirmEmailAsync(string token);
        Task<IActionResult> LoginUserAsync(LoginRequest request);
        Task<IActionResult> ResendVerificationTokenAsync(string email);
        Task<IActionResult> RequestPasswordResetAsync(string email);
        Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest request);
        Task<IActionResult> LoginWithFacebookAsync(ExternalLoginInfoModel externalInfo);
        Task<IActionResult> ChangePasswordAsync(string userId, ChangePasswordRequest request);

    }
}
