using DrugPreventionSystemBE.DrugPreventionSystem.Enity;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView;
using Microsoft.AspNetCore.Mvc;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public interface IAuthenticationService
    {
        Task<IActionResult> RegisterUserAsync(UserRegisterRequest request);
        Task<IActionResult> ConfirmEmailAsync(string token);
        Task<IActionResult> LoginUserAsync(LoginRequest request);
        Task<IActionResult> ResendVerificationTokenAsync(string email);
    }
}
