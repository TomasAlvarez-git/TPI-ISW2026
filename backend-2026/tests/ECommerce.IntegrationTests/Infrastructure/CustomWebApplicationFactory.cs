using System.Data.Common;
using Dsw2025Tpi.Api;
using Dsw2025Tpi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remover las opciones y configuraciones de DbContext existentes
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(DbContextOptions<Dsw2025TpiContext>) ||
                    d.ServiceType == typeof(DbContextOptions<AuthenticateContext>) ||
                    d.ServiceType == typeof(Dsw2025TpiContext) ||
                    d.ServiceType == typeof(AuthenticateContext) ||
                    (d.ServiceType.FullName != null && d.ServiceType.FullName.StartsWith("Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration"))
                )
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Crear y mantener abierta la conexión SQLite en memoria compartida
            if (_connection == null)
            {
                _connection = new SqliteConnection("Data Source=InMemoryDb;Mode=Memory;Cache=Shared");
                _connection.Open();
            }

            // Registrar DbContexts configurados con SQLite
            services.AddDbContext<Dsw2025TpiContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            services.AddDbContext<AuthenticateContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Asegurar la creación de esquemas en SQLite antes del inicio y seeding de roles
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var authContext = scopedServices.GetRequiredService<AuthenticateContext>();
            authContext.Database.EnsureCreated();

            var appContext = scopedServices.GetRequiredService<Dsw2025TpiContext>();
            var relationalCreator = appContext.GetService<IRelationalDatabaseCreator>();
            try
            {
                relationalCreator.CreateTables();
            }
            catch
            {
                appContext.Database.EnsureCreated();
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
        }
    }
}

