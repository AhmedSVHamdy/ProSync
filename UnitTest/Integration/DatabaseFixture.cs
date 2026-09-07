using Infrastructure;
using Infrastructure.ApplicationDbContext;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using Testcontainers.MsSql;
using Xunit;

namespace UnitTest.Integration
{
    public class DatabaseFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _sqlContainer;
        public ProSyncContext DbContext { get; private set; } = null!;

        public DatabaseFixture()
        {
            _sqlContainer = new MsSqlBuilder()
                .WithPassword("YourStrong@Passw0rd")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _sqlContainer.StartAsync();

            var options = new DbContextOptionsBuilder<ProSyncContext>()
                .UseSqlServer(_sqlContainer.GetConnectionString())
                .Options;

            var tenantProvider = new TenantProviderRepositories();   // نفس الـ Implementation الحقيقية بتاعتك

            DbContext = new ProSyncContext(options, tenantProvider);
            await DbContext.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _sqlContainer.DisposeAsync();
        }
    }
}

