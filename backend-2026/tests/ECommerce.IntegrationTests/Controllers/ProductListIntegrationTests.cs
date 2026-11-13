using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using ECommerce.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ECommerce.IntegrationTests.Controllers
{
    [Collection("IntegrationTests")]
    public class ProductListIntegrationTests
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ProductListIntegrationTests(CustomWebApplicationFactory factory)
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
                        throw new Exception($"Fallo al crear rol ADMIN: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }

                var adminUser = await userManager.FindByNameAsync("adminuser");
                if (adminUser == null)
                {
                    adminUser = new IdentityUser { UserName = "adminuser", Email = "admin@admin.com" };
                    var createResult = await userManager.CreateAsync(adminUser, "AdminPassword123!");
                    if (!createResult.Succeeded)
                        throw new Exception($"Fallo al crear usuario admin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

                    var roleAssignResult = await userManager.AddToRoleAsync(adminUser, "ADMIN");
                    if (!roleAssignResult.Succeeded)
                        throw new Exception($"Fallo al asignar rol ADMIN: {string.Join(", ", roleAssignResult.Errors.Select(e => e.Description))}");
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
        public async Task GetProducts_PublicEndpoint_ReturnsOkWithPaginatedJsonStructure()
        {
            // Arrange 
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert – debe ser 200 OK (el servicio siempre devuelve un ResponsePagination no-null)
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadAsStringAsync();
            body.Should().NotBeNullOrEmpty();

            var pagination = JsonSerializer.Deserialize<ProductModel.ResponsePagination>(body, JsonOptions);
            pagination.Should().NotBeNull();
            pagination!.Products.Should().NotBeNull();         
            pagination.TotalFiltered.Should().BeGreaterThanOrEqualTo(0);
            pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task GetProducts_WithQueryStringFilters_ReturnsFilteredResultsFromSqlite()
        {
            // Arrange – sembrar 3 productos activos con nombres distinguibles
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();

                if (!await db.Products.AnyAsync(p => p.Sku == "SKU-7001"))
                {
                    db.Products.AddRange(
                        new Product("SKU-7001", "INT-7001", "Monitor LED 24", "Panel IPS Full HD", 250.0m, 15),
                        new Product("SKU-7002", "INT-7002", "Monitor Curvo 27", "Panel VA 144Hz", 400.0m, 8),
                        new Product("SKU-7003", "INT-7003", "Teclado Slim", "Perfil bajo USB", 60.0m, 30)
                    );
                    await db.SaveChangesAsync();
                }
            }

            _client.DefaultRequestHeaders.Authorization = null;

            // Act – buscar "Monitor"
            var response = await _client.GetAsync("/api/products?search=Monitor&pageNumber=1&pageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var pagination = await response.Content.ReadFromJsonAsync<ProductModel.ResponsePagination>(JsonOptions);
            pagination.Should().NotBeNull();
            pagination!.Products.Should().HaveCount(2);
            pagination.Products.Should().OnlyContain(p => p.Name!.Contains("Monitor"));
            pagination.TotalFiltered.Should().Be(2);
        }

        [Fact]
        public async Task GetAuthProducts_AdminEndpoint_WhenCalledByAdmin_IncludesInactiveProducts()
        {
            // Arrange – sembrar un producto inactivo directamente en SQLite
            var inactiveSku = "SKU-7099";
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();

                if (!await db.Products.AnyAsync(p => p.Sku == inactiveSku))
                {
                    // Crear con stock > 0 para evitar excepción y luego marcar inactivo
                    var inactiveProduct = new Product(inactiveSku, "INT-7099", "Producto Descontinuado", "Solo visible para admin", 99.0m, 5);
                    inactiveProduct.IsActive = false;
                    db.Products.Add(inactiveProduct);
                    await db.SaveChangesAsync();
                }
            }

            await AuthenticateAsAdminAsync();

            // Act – GET al endpoint administrativo
            var response = await _client.GetAsync("/api/products/admin");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var pagination = await response.Content.ReadFromJsonAsync<ProductModel.ResponsePagination>(JsonOptions);
            pagination.Should().NotBeNull();
            pagination!.Products.Should().NotBeEmpty();

            // Verificar que el producto inactivo está incluido en la respuesta
            var inactiveInResponse = pagination.Products.FirstOrDefault(p => p.Sku == inactiveSku);
            inactiveInResponse.Should().NotBeNull("el endpoint admin debe incluir productos inactivos");
            inactiveInResponse!.IsActive.Should().BeFalse();
        }
    }
}

