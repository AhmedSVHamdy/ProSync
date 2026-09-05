using Core.Domain.Entities;
using Core.Domain.Features.Projects.Commands.MultiTenancy;
using Core.Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Interceptors
{
    public class AuditLogInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context is null)
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            var auditEntries = new List<AuditLog>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                // فاكر ليه بنستثني AuditLog نفسها؟ عشان منعملش Loop لا نهائي (نسجل سجل عن سجل عن سجل...)
                if (entry.Entity is AuditLog || entry.State == EntityState.Unchanged || entry.State == EntityState.Detached)
                    continue;

                if (entry.Entity is not TenantEntity tenantEntity)
                    continue;   // بنسجل بس على الكيانات اللي عندها Tenant، مش كل حاجة في النظام

                var action = entry.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Modified => "Updated",
                    EntityState.Deleted => "Deleted",
                    _ => null
                };

                if (action is null)
                    continue;

                var currentUserId = TenantProviderAccessor.CurrentUserId;   // هنضيفها دلوقتي (شرح تحت)

                auditEntries.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantEntity.TenantId,
                    UserId = currentUserId ?? Guid.Empty,   // فاكر ليه ممكن تكون فاضية؟ لو العملية جاية من Background Job (Hangfire/Webhook)
                    Action = $"{action} {entry.Entity.GetType().Name}",
                    Timestamp = DateTime.UtcNow,
                    TaskItemId = entry.Entity is TaskItem task ? task.Id : null
                });
            }

            if (auditEntries.Count > 0)
            {
                context.Set<AuditLog>().AddRange(auditEntries);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
