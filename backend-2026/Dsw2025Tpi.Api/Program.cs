
using System.Text;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Api.Configurations;

using Microsoft.AspNetCore.Identity;
using Serilog;
using Microsoft.Extensions.Configuration;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build())
            .Enrich.FromLogContext()
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();

        builder.Services.AddRateLimiterService();

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerConfiguration();

        builder.Services.AddIdentityAndAuthenticationServices(builder.Configuration);

        builder.Services.AddScoped<IAuthenticateService, AuthenticateService>();

        builder.Services.AddHealthChecks();

        builder.Services.AddDomainServices(builder.Configuration);

        builder.Services.AddCorsService();

        builder.Services.AddTransient<CustomExceptionHandlingMiddleware>();

        var app = builder.Build();

        var rolesToCreate = builder.Configuration.GetSection("Roles").Get<List<string>>();

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in rolesToCreate!)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var admin = builder.Configuration.GetSection("DefaultAdmin");
            var adminEmail = admin.GetValue<string>("email");
            var adminPassword = admin.GetValue<string>("password");
            var adminName = admin.GetValue<string>("username");

            UserManager<IdentityUser> manager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var userAdmin = await manager.FindByNameAsync(adminName!);
            if (userAdmin is null)
            {
                var user = new IdentityUser
                {
                    UserName = adminName,
                    Email = adminEmail
                };

                var result = await manager.CreateAsync(user, adminPassword!);

                var roleResult = await manager.AddToRoleAsync(user, "ADMIN");

                if (!result.Succeeded || !roleResult.Succeeded)
                {
                    Log.Error(string.Join(", ", result.Errors.Select(e => e.Description)));
                    throw new Exception();
                }
            }
        }
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors("PermitirFrontend");
        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseAuthorization();

        app.UseMiddleware<CustomExceptionHandlingMiddleware>();
        app.MapControllers().RequireRateLimiting("fixed");


        app.MapHealthChecks("/health-check");

        app.Run();
    }
}
