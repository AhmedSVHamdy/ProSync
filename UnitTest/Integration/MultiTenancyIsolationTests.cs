using Core.Domain.Entities;
using Core.Domain.Features.Projects.Commands.MultiTenancy;
using Core.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Integration
{
    public class MultiTenancyIsolationTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public MultiTenancyIsolationTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Users_FromDifferentTenants_AreIsolatedByQueryFilter()
        {
            var context = _fixture.DbContext;

            var tenantA = new Tenant { Id = Guid.NewGuid(), Name = "Company A", PlanType = "Free" };
            var tenantB = new Tenant { Id = Guid.NewGuid(), Name = "Company B", PlanType = "Free" };

            var userA = new User { Id = Guid.NewGuid(), TenantId = tenantA.Id, Name = "Ahmed", Email = "ahmed@a.com", PasswordHash = "hash", Role = UserRole.Owner.ToString(), IsEmailVerified = true, IsActive = true };
            var userB = new User { Id = Guid.NewGuid(), TenantId = tenantB.Id, Name = "Mohamed", Email = "mohamed@b.com", PasswordHash = "hash", Role = UserRole.Owner.ToString(), IsEmailVerified = true, IsActive = true };

            context.Tenants.AddRange(tenantA, tenantB);
            context.Users.AddRange(userA, userB);
            await context.SaveChangesAsync();

            TenantProviderAccessor.TenantId = tenantA.Id;

            var visibleUsers = await context.Users.ToListAsync();

            visibleUsers.Should().ContainSingle();
            visibleUsers.Should().Contain(u => u.Id == userA.Id);
            visibleUsers.Should().NotContain(u => u.Id == userB.Id);
        }
    }
}
