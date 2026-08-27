using Core.DTO.Authentication;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        /// <summary>
        /// Registers a new user and sends an OTP to their email for verification.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }
        /// <summary>
        /// Verifies the OTP sent to the user's email during registration.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto)
        {
            await _authService.VerifyOtpAsync(dto);
            return Ok(new { message = "تم تفعيل بريدك الإلكتروني بنجاح." });
        }
        /// <summary>
        /// Resends the OTP to the user's email if they didn't receive it or if it expired.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto dto)
        {
            await _authService.ResendOtpAsync(dto);
            return Ok(new { message = "إذا كان بريدك الإلكتروني مسجلاً، فسيصلك كود تحقق جديد." });
        }
        /// <summary>
        /// Authenticates a user and returns an access token and refresh token upon successful login.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        /// <summary>
        /// Refreshes the access token using a valid refresh token.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto);
            return Ok(result);
        }
        /// <summary>
        /// Logs out the user by invalidating the provided refresh token.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
        {
            var userId = GetCurrentUserId();
            await _authService.LogoutAsync(userId, dto.RefreshToken);
            return Ok(new { message = "تم تسجيل الخروج بنجاح." });
        }
        /// <summary>
        /// Initiates the password reset process by sending a reset code to the user's email if the email is registered.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            await _authService.ForgotPasswordAsync(dto);
            return Ok(new { message = "إذا كان بريدك الإلكتروني مسجلاً، فسيصلك كود إعادة تعيين كلمة المرور." });
        }
        /// <summary>
        /// Resets the user's password using the provided reset code and new password.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok(new { message = "تم إعادة تعيين كلمة المرور بنجاح." });
        }
        /// <summary>
        /// Changes the password for the currently authenticated user.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            var userId = GetCurrentUserId();
            await _authService.ChangePasswordAsync(userId, dto);
            return Ok(new { message = "تم تغيير كلمة المرور بنجاح." });
        }
        /// <summary>
        /// Retrieves the details of the currently authenticated user.
        /// </summary>
        /// <returns></returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetCurrentUserId();
            var result = await _authService.GetMeAsync(userId);
            return Ok(result);
        }
        /// <summary>
        /// Deletes the account of the currently authenticated user.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequestDto dto)
        {
            var userId = GetCurrentUserId();
            await _authService.DeleteAccountAsync(userId, dto);
            return Ok(new { message = "تم حذف الحساب بنجاح." });
        }
        /// <summary>
        /// Authenticates a user using their Google account. 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
        {
            var result = await _authService.GoogleLoginAsync(dto);
            return Ok(result);
        }

        // Method خاصة مشتركة، بتقرا الـ UserId من الـ JWT Claims بتاعة اليوزر المسجل دخول
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("لم يتم التعرف على المستخدم.");

            return userId;
        }
    }
}

