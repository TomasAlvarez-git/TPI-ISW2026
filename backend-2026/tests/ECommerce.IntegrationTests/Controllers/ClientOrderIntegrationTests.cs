using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
    public class ClientOrderIntegrationTests
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public ClientOrderIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<Customer> AuthenticateAsUserAsync()
        {
            Customer? customer;
            using (var scope = _factory.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();

                if (!await roleManager.RoleExistsAsync("USER"))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole("USER"));
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception($"Fallo al crear rol USER: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }

                customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.Email == "orderclient@test.com");
                if (customer == null)
                {
                    customer = new Customer("orderclient@test.com", "Order Client", "123456789");
                    dbContext.Customers.Add(customer);
                    await dbContext.SaveChangesAsync();
                }

                var user = await userManager.FindByNameAsync("orderclient");
                if (user == null)
                {
                    user = new IdentityUser { UserName = "orderclient", Email = "orderclient@test.com" };
                    var createResult = await userManager.CreateAsync(user, "UserPassword123!");
                    if (!createResult.Succeeded)
                    {
                        throw new Exception($"Fallo al crear usuario cliente: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }

                    var roleAssignResult = await userManager.AddToRoleAsync(user, "USER");
                    if (!roleAssignResult.Succeeded)
                    {
                        throw new Exception($"Fallo al asignar rol USER: {string.Join(", ", roleAssignResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            var loginModel = new LoginModel.RequestLogin("orderclient", "UserPassword123!");
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginModel);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Login como USER falló con status {response.StatusCode}. Detalle: {error}");
            }

            var authResponse = await response.Content.ReadFromJsonAsync<LoginModel.ResponseLogin>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);

            return customer;
        }

        [Fact]
        public async Task CreateOrder_WhenUnauthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            var payload = new OrderModel.RequestOrder(
                "Avenida Siempre Viva 742",
                "Avenida Siempre Viva 742",
                "Dejar en porteria",
                Guid.NewGuid(),
                new List<OrderItemModel.RequestItem>
                {
                    new OrderItemModel.RequestItem(Guid.NewGuid(), 1)
                }
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", payload);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateOrder_WhenAuthenticatedWithValidData_ReturnsCreatedAndPersistsOrderAndStockInSqlite()
        {
            // Arrange
            var customer = await AuthenticateAsUserAsync();

            var product = new Product("SKU-6101", "INT-6101", "Auriculares Pro", "Auriculares con cancelacion de ruido", 80.0m, 10);
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync();
            }

            var orderPayload = new OrderModel.RequestOrder(
                "Calle San Martin 123",
                "Calle San Martin 123",
                "Entregar de tarde",
                customer.Id,
                new List<OrderItemModel.RequestItem>
                {
                    new OrderItemModel.RequestItem(product.Id, 3)
                }
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", orderPayload);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var createdOrder = await response.Content.ReadFromJsonAsync<OrderModel.ResponseOrder>();
            createdOrder.Should().NotBeNull();
            response.Headers.Location!.ToString().Should().Contain($"/api/orders/{createdOrder!.Id}");

            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();

                var orderInDb = await dbContext.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == createdOrder.Id);

                orderInDb.Should().NotBeNull();
                orderInDb!.CustomerId.Should().Be(customer.Id);
                orderInDb.OrderItems.Should().HaveCount(1);
                orderInDb.OrderItems.First().ProductId.Should().Be(product.Id);
                orderInDb.OrderItems.First().Quantity.Should().Be(3);

                var productInDb = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
                productInDb.Should().NotBeNull();
                productInDb!.StockQuantity.Should().Be(7);
            }
        }

        [Fact]
        public async Task CreateOrder_WhenStockIsInsufficient_ReturnsBadRequestWithErrorMessage()
        {
            // Arrange
            var customer = await AuthenticateAsUserAsync();

            var product = new Product("SKU-6201", "INT-6201", "Mouse Inalambrico", "Mouse optico USB", 35.0m, 4);
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync();
            }

            var orderPayload = new OrderModel.RequestOrder(
                "Avenida Libertador 456",
                "Avenida Libertador 456",
                "Pedido con sobrestock",
                customer.Id,
                new List<OrderItemModel.RequestItem>
                {
                    new OrderItemModel.RequestItem(product.Id, 10)
                }
            );

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", orderPayload);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var errorContent = await response.Content.ReadAsStringAsync();
            errorContent.Should().NotBeNullOrEmpty();
            errorContent.ToLower().Should().Contain("stock");
        }
    }
}

