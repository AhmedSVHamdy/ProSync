using Core.ServiceContracts;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            var subject = "تأكيد بريدك الإلكتروني - ProSync";
            var body = $"كود التأكيد بتاعك هو: <b>{otpCode}</b><br/>الكود صالح لمدة 5 دقائق.";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string otpCode)
        {
            var subject = "إعادة تعيين كلمة المرور - ProSync";
            var body = $"كود إعادة تعيين كلمة المرور: <b>{otpCode}</b><br/>الكود صالح لمدة 5 دقائق.";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendInvitationEmailAsync(string toEmail, string invitationLink, string tenantName)
        {
            var subject = $"دعوة للانضمام إلى {tenantName} على ProSync";
            var body = $"تمت دعوتك للانضمام إلى فريق {tenantName}.<br/>" +
                       $"اضغط على الرابط للانضمام: <a href='{invitationLink}'>{invitationLink}</a>";

            await SendEmailAsync(toEmail, subject, body);
        }

        // Method خاصة (private) تجمع منطق الإرسال المشترك بين الـ 3 methods فوق
        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["EmailSettings:SenderName"],
                _configuration["EmailSettings:SenderEmail"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _configuration["EmailSettings:SmtpServer"],
                int.Parse(_configuration["EmailSettings:SmtpPort"]!),
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                _configuration["EmailSettings:SenderEmail"],
                _configuration["EmailSettings:SenderPassword"]);   // ده هو اللي محطوطه في User Secrets

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
