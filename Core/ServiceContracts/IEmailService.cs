using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ServiceContracts
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode);
        Task SendPasswordResetEmailAsync(string toEmail, string otpCode);
        Task SendInvitationEmailAsync(string toEmail, string invitationLink, string tenantName);
    }
}
