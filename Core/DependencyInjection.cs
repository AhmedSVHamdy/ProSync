using Core.Domain.Features.Projects.Commands.CreateProject;
using Core.Domain.RepositoryContracts;
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
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenIssuerService, TokenIssuerService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IInvitationService, InvitationService>();
            services.AddScoped<ITokenIssuerService, TokenIssuerService>();
            services.AddScoped<ICacheService, RedisCacheService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<INotificationService, NotificationService>();



            services.AddValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>();
            services.AddAutoMapper(cfg => { }, typeof(AuthMappingProfile).Assembly);
            // في WebApi أو Infrastructure DI
             services.AddMediatR(cfg =>
             cfg.RegisterServicesFromAssembly(typeof(CreateProjectCommand).Assembly));
            return services;
        }
    }
}
