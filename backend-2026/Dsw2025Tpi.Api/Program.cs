
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
                .AddJsonFile("appsettings.Development.json")
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

        await builder.Services.CreateRoles(app, builder.Configuration);

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
