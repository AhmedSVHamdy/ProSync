using Core.Domain.Entities;
using Core.Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Infrastructure.ApplicationDbContext
{
    public partial class ProSyncContext : DbContext
    {
        private readonly ITenantProviderRepository _tenantProvider;
       
        public ProSyncContext(DbContextOptions<ProSyncContext> options, ITenantProviderRepository tenantProvider) : base(options)
        {
            _tenantProvider = tenantProvider;
        }
        public Guid CurrentTenantId => _tenantProvider.TenantId ?? Guid.Empty;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. تطبيق كل كلاسات الـ Configuration من الـ Assembly تلقائياً
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProSyncContext).Assembly);

            // 2. تطبيق الـ Query Filter ديناميكياً لعزل بيانات الـ Tenants
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var filter = Expression.Lambda(
                        Expression.Equal(
                            Expression.Property(parameter, nameof(TenantEntity.TenantId)),
                            Expression.Property(Expression.Constant(this), nameof(CurrentTenantId))
                        ),
                        parameter
                    );

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }

            // 🚀 3. الحل القاطع لإيرور الـ Foreign Key Cycle: تحويل كل الـ Cascade Delete إلى NoAction
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.NoAction;
            }

            OnModelCreatingPartial(modelBuilder);
        }







        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Sprint> Sprints { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<UserSettings> UserSettings { get; set; } = null!;
        public DbSet<Tenant> Tenants { get; set; } = null!;
        public DbSet<TaskItem> TaskItems { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        


        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
    
}
