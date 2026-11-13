using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Data;
using ECommerce.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.IntegrationTests.Controllers
{
    [Collection("IntegrationTests")]
    public class AdminProductIntegrationTests
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AdminProductIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task AuthenticateAsAdminAsync()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                if (!await roleManager.RoleExistsAsync("ADMIN"))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole("ADMIN"));
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception($"Fallo al crear rol ADMIN: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }

                var adminUser = await userManager.FindByNameAsync("adminuser");
                if (adminUser == null)
                {
                    adminUser = new IdentityUser { UserName = "adminuser", Email = "admin@admin.com" };
                    var createResult = await userManager.CreateAsync(adminUser, "AdminPassword123!");
                    if (!createResult.Succeeded)
                    {
                        throw new Exception($"Fallo al crear usuario admin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }

                    var roleAssignResult = await userManager.AddToRoleAsync(adminUser, "ADMIN");
                    if (!roleAssignResult.Succeeded)
                    {
                        throw new Exception($"Fallo al asignar rol ADMIN al usuario: {string.Join(", ", roleAssignResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            var loginModel = new LoginModel.RequestLogin("adminuser", "AdminPassword123!");

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginModel);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Login falló con código {response.StatusCode}. Detalle: {errorContent}");
            }

            var authResponse = await response.Content.ReadFromJsonAsync<LoginModel.ResponseLogin>(); 
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);
        }

        [Fact]
        public async Task AddProduct_WhenUnauthenticated_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var requestPayload = new ProductModel.RequestProduct(
                "TEST-NOAUTH-01",
                "NA-001",
                "Producto Sin Auth",
                "No debería guardarse",
                100.0m,
                10
            );

            var response = await _client.PostAsJsonAsync("/api/products", requestPayload);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AddProduct_WhenAuthenticatedAsAdmin_PersistsInSqliteAndReturnsCreated()
        {
            await AuthenticateAsAdminAsync(); 

            var requestPayload = new ProductModel.RequestProduct(
                "SKU-9001",
                "INT-9001",
                "Teclado Mecanico",
                "Teclado RGB para pruebas",
                150.5m,
                50
            );

            var response = await _client.PostAsJsonAsync("/api/products", requestPayload);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            
            var productInDb = await dbContext.Products.FirstOrDefaultAsync(p => p.Sku == "SKU-9001");
            
            productInDb.Should().NotBeNull();
            productInDb!.Name.Should().Be("Teclado Mecanico");
            productInDb.CurrentUnitPrice.Should().Be(150.5m);
            productInDb.StockQuantity.Should().Be(50);
            productInDb.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task AddProduct_WhenSkuAlreadyExistsInDatabase_ReturnsConflictStatusCode()
        {
            var existingSku = "SKU-DUP-999";
            
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
                dbContext.Products.Add(new Product 
                { 
                    Id = Guid.NewGuid(),
                    Sku = existingSku, 
                    InternalCode = "EXT-001",
                    Name = "Producto Original", 
                    Description = "Sembrado manual",
                    CurrentUnitPrice = 50.0m,
                    StockQuantity = 10,
                    IsActive = true
                });
                await dbContext.SaveChangesAsync();
            }

            await AuthenticateAsAdminAsync();

            var duplicateRequestPayload = new ProductModel.RequestProduct(
                existingSku, 
                "EXT-002",
                "Intento Duplicado",
                "Debería fallar",
                80.0m,
                5
            );

            var response = await _client.PostAsJsonAsync("/api/products", duplicateRequestPayload);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
        }
    }
}