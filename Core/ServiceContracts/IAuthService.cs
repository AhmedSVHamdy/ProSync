using Core.DTO.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
        Task LogoutAsync(Guid userId, string refreshToken);

        Task VerifyOtpAsync(VerifyOtpRequestDto dto);
        Task ResendOtpAsync(ResendOtpRequestDto dto);

        Task ForgotPasswordAsync(ForgotPasswordRequestDto dto);
        Task ResetPasswordAsync(ResetPasswordRequestDto dto);
        Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto dto);

        Task<UserProfileResponseDto> GetMeAsync(Guid userId);
        Task DeleteAccountAsync(Guid userId, DeleteAccountRequestDto dto);

        Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto dto);
    }
}
