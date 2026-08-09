using Core.Mappers;
using Core.ServiceContracts;
using Core.ServiceContracts.Core.Application.Contracts.Services;
using Core.Services;
using Core.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IGoogleAuthValidator, GoogleAuthValidator>();
            services.AddScoped<IAdminUserService, AdminUserService>();


            services.AddValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>();
            services.AddAutoMapper(cfg => { }, typeof(AuthMappingProfile).Assembly);
            return services;
        }
    }
}
