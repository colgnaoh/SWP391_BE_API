using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.AuthModel;
using DrugPreventionSystemBE.DrugPreventionSystem.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Mvc;


namespace DrugPreventionSystemBE.DrugPreventionSystem.Controller
{

    [ApiController]
    [Route("/api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly Service.IAuthenticationService _authenticationService;
        public AuthController(Service.IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            return await _authenticationService.RegisterUserAsync(request);
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            return await _authenticationService.ConfirmEmailAsync(token);
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            return await _authenticationService.LoginUserAsync(request);
        }

        [HttpPost("resend-token")]
        public async Task<IActionResult> ResendToken([FromBody] ResendTokenRequest request)
        {
            // Gọi phương thức ResendVerificationTokenAsync từ service
            return await _authenticationService.ResendVerificationTokenAsync(request.Email);
        }

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ForgotPasswordRequest request)
        {
            // Gọi phương thức RequestPasswordResetAsync từ service
            return await _authenticationService.RequestPasswordResetAsync(request.Email);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            return await _authenticationService.ResetPasswordAsync(request);
        }

        [HttpGet("facebook-login")]
        public IActionResult FacebookLogin(string? returnUrl = "/")
        {
            var properties = new AuthenticationProperties { RedirectUri = returnUrl };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }
        //https://your-backend-domain/auth/facebook-login?returnUrl=https://your-frontend-domain



    }
}
