using Core.ServiceContracts;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Services
{
    public class GoogleAuthValidator : IGoogleAuthValidator
    {
        private readonly IConfiguration _configuration;

        public GoogleAuthValidator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GooglePayload?> ValidateAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

                return new GooglePayload
                {
                    Email = payload.Email,
                    Name = payload.Name
                };
            }
            catch (InvalidJwtException)
            {
                return null;   // التوكن مزور أو منتهي أو مش صادر من الـ Client ID بتاعنا
            }
        }
    }
}
