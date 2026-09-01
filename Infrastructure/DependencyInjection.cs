using Core.Domain.RepositoryContracts;
using Core.ServiceContracts;
using Core.Services;
using Hangfire;
using Infrastructure.ApplicationDbContext;
using Infrastructure.Interceptors;
using Infrastructure.Repositories;
using Infrastructure.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ====================================================
            // 1. إعدادات قواعد البيانات (SQL)
            // ====================================================
            services.AddDbContext<ProSyncContext>((serviceProvider ,options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });
                options.AddInterceptors(new AuditLogInterceptor());
            });

            // ====================================================
            // 3. إعدادات Hangfire (العسكري اللي مش بينام)
            // ====================================================
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                // بيستخدم نفس الـ ConnectionString بتاع المشروع
                .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));

            // تشغيل السيرفر الداخلي لـ Hangfire عشان يبدأ ينفذ المهام
            services.AddHangfireServer();


            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ITenantProviderRepository, TenantProviderRepositories>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IInvitationRepository, InvitationRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ISprintRepository, SprintRepository>();
            services.AddScoped<ITaskItemRepository, TaskItemRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
            services.AddScoped<ITaskNotifier, SignalRTaskNotifier>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddSignalR();

            return services;
        }
       
    }
}
