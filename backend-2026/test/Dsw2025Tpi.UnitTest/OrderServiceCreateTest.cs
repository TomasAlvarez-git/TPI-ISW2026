using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Application.Services;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;

namespace Dsw2025Tpi.UnitTest;

public class OrderServiceCreateTest
{

    [Fact]
    public async Task CreateOrder_WithValidItemsAndStock_CalculatesTotalAndDecrementsProductStock(){

        // 1. ARRANGE (Preparar dependencias y mocks)
            var repoMock = new Mock<IRepository>();
            var clienteMock = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Angeles Navarro",
                Email = "anguie@gmail.com",
                PhoneNumber = "3810000001",
            };
            var productsInDb = new List<Product>
            {
                new Product
                {
                    Sku = "SKU-0001",
                    InternalCode = "INT-0001",
                    Name = "Auriculares con cancelacion de ruido",
                    Description = "Auriculares con cancelacion de ruido",
                    CurrentUnitPrice = 1200,
                    StockQuantity = 20,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0002",
                    InternalCode = "INT-0002",
                    Name = "Auriculares sin cancelacion de ruido",
                    Description = "Auriculares sin cancelacion de ruido",
                    CurrentUnitPrice = 1200,
                    StockQuantity = 20,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0003",
                    InternalCode = "INT-0003",
                    Name = "Mouse bluetooth",
                    Description = "",
                    CurrentUnitPrice = 1200,
                    StockQuantity = 20,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0004",
                    InternalCode = "INT-0004",
                    Name = "Auriculares bluetooth",
                    Description = "",
                    CurrentUnitPrice = 1200,
                    StockQuantity = 20,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0005",
                    InternalCode = "INT-0005",
                    Name = "Mouse inalámbrico ergonómico",
                    Description = "Mouse óptico inalámbrico con receptor USB y diseño ergonómico",
                    CurrentUnitPrice = 2200,
                    StockQuantity = 30,
                    IsActive = true
                }
            };
            var ordenNuevo = new Order{
                ShippingAddress = "Calle Mitre 123",
                BillingAddress = "Calle Mitre 123",
                Notes = "Sin notas",
                CustomerId = clienteMock.Id,
                OrderItems = [new OrderItem {
                    ProductId = productsInDb[0].Id,
                    Quantity = 2
                }]
            };

            var productoIdEsperado = Guid.NewGuid();
            var orderIdEsperada = Guid.NewGuid();

            repoMock.Setup(r => r.GetById<Product>(productsInDb[0].Id))
                .ReturnsAsync(productsInDb[0]);
            repoMock.Setup(r => r.First(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                    .ReturnsAsync(productsInDb[0]);
            repoMock.Setup(r => r.First(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>()))
                    .ReturnsAsync(clienteMock);
            repoMock.Setup(r => r.GetById<Customer>(clienteMock.Id))
                    .ReturnsAsync(clienteMock);
            var loggerMock = new Mock<ILogger<OrdersManagementService>>();
            repoMock.Setup(r => r.Add(It.IsAny<Order>()))
                    .ReturnsAsync((Order o) => new Order
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = clienteMock.Id,
                        ShippingAddress = "Calle Mitre 123",
                        BillingAddress = "Calle Mitre 123",
                        Notes = "Sin notas",
                        OrderItems = [new OrderItem {
                            ProductId = productsInDb[0].Id,
                            Quantity = 2
                    }]
                    });

            var service = new OrdersManagementService(repoMock.Object, loggerMock.Object);

            // 2. ACT (Ejecutar la unidad)
            var orderRequest = new OrderModel.RequestOrder(ordenNuevo.ShippingAddress, ordenNuevo.BillingAddress, ordenNuevo.Notes, ordenNuevo.CustomerId, [new OrderItemModel.RequestItem(productsInDb[0].Id, 2)]);
            var resultado = await service.AddOrder(orderRequest);

            // 3. ASSERT (Verificar resultados)

            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.OrderItems[0].Quantity);  
            Assert.NotEqual(Guid.Empty, resultado.Id);
            Assert.Equal(clienteMock.Id, resultado.CustomerId);
            Assert.Equal("Calle Mitre 123", resultado.ShippingAddress);
            Assert.Equal("Calle Mitre 123", resultado.BillingAddress);
            Assert.Equal("Sin notas", resultado.Notes);    
            Assert.Equal(2400, resultado.TotalAmount);
            Assert.Equal(productsInDb[0].Id, resultado.OrderItems[0].ProductId);
            Assert.Equal(productsInDb[0].Name, resultado.OrderItems[0].Name);
            Assert.Equal(productsInDb[0].CurrentUnitPrice, resultado.OrderItems[0].UnitPrice);
            Assert.Equal(18, productsInDb[0].StockQuantity);
            Assert.NotEqual(Guid.Empty, resultado.OrderItems[0].ProductId);

            repoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_WhenProductStockIsInsufficient_ThrowsInsufficientStockException(){
        var repoMock = new Mock<IRepository>();
        var clienteMock = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Angeles Navarro",
            Email = "anguie@gmail.com",
            PhoneNumber = "3810000001",
        };
        var productsInDb = new List<Product>
        {
            new Product
            {
                Sku = "SKU-0001",
                InternalCode = "INT-0001",
                Name = "Auriculares con cancelacion de ruido",
                Description = "Auriculares con cancelacion de ruido",
                CurrentUnitPrice = 1200,
                StockQuantity = 1,
                IsActive = true
            }
        };
        var ordenNuevo = new Order{
            ShippingAddress = "Calle Mitre 123",
            BillingAddress = "Calle Mitre 123",
            Notes = "Sin notas",
            CustomerId = clienteMock.Id,
            OrderItems = [new OrderItem {
                ProductId = productsInDb[0].Id,
                Quantity = 10
            }]
        };

        var productoIdEsperado = Guid.NewGuid();
        var orderIdEsperada = Guid.NewGuid();

        repoMock.Setup(r => r.GetById<Product>(productsInDb[0].Id))
                .ReturnsAsync(productsInDb[0]);
        repoMock.Setup(r => r.First(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .ReturnsAsync(productsInDb[0]);
        repoMock.Setup(r => r.First(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>()))
                .ReturnsAsync(clienteMock);
        repoMock.Setup(r => r.GetById<Customer>(clienteMock.Id))
                .ReturnsAsync(clienteMock);
        var loggerMock = new Mock<ILogger<OrdersManagementService>>();
        repoMock.Setup(r => r.Add(It.IsAny<Order>()))
                .ReturnsAsync((Order o) => new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = clienteMock.Id,
                    ShippingAddress = "Calle Mitre 123",
                    BillingAddress = "Calle Mitre 123",
                    Notes = "Sin notas",
                    OrderItems = [new OrderItem {
                        ProductId = productsInDb[0].Id,
                        Quantity = 10
                }]
                });

        var service = new OrdersManagementService(repoMock.Object, loggerMock.Object);

        // 2. ACT (Ejecutar la unidad)
        var orderRequest = new OrderModel.RequestOrder(ordenNuevo.ShippingAddress, ordenNuevo.BillingAddress, ordenNuevo.Notes, ordenNuevo.CustomerId, [new OrderItemModel.RequestItem(productsInDb[0].Id, 10)]);

        // 3. ASSERT (Verificar resultados)

        var ex = await Assert.ThrowsAsync<System.ApplicationException>(() => service.AddOrder(orderRequest));
        Assert.Equal($"No hay suficiente stock, el producto {productsInDb[0].Id} tiene {productsInDb[0].StockQuantity} items en existencia", ex.Message);
        repoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
    }
    
    [Fact]
    public async Task   CreateOrder_WhenCustomerDoesNotExist_ThrowsCustomerNotFoundException(){
        var repoMock = new Mock<IRepository>();
        var clienteMock = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Angeles Navarro",
            Email = "anguie@gmail.com",
            PhoneNumber = "3810000001",
        };
        var productsInDb = new List<Product>
        {
            new Product
            {
                Sku = "SKU-0001",
                InternalCode = "INT-0001",
                Name = "Auriculares con cancelacion de ruido",
                Description = "Auriculares con cancelacion de ruido",
                CurrentUnitPrice = 1200,
                StockQuantity = 1,
                IsActive = true
            }
        };

        var clienteInexistenteId = Guid.NewGuid();

        var ordenNuevo = new Order{
            ShippingAddress = "Calle Mitre 123",
            BillingAddress = "Calle Mitre 123",
            Notes = "Sin notas",
            CustomerId = clienteInexistenteId,
            OrderItems = [new OrderItem {
                ProductId = productsInDb[0].Id,
                Quantity = 1
            }]
        };


        var productoIdEsperado = Guid.NewGuid();
        var orderIdEsperada = Guid.NewGuid();

        repoMock.Setup(r => r.GetById<Product>(productsInDb[0].Id))
                .ReturnsAsync(productsInDb[0]);
        repoMock.Setup(r => r.First(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .ReturnsAsync(productsInDb[0]);
        repoMock.Setup(r => r.First(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>()))
                .ReturnsAsync((Customer?)null);
        repoMock.Setup(r => r.GetById<Customer>(clienteInexistenteId))
                .ReturnsAsync((Customer?)null);
        var loggerMock = new Mock<ILogger<OrdersManagementService>>();
        repoMock.Setup(r => r.Add(It.IsAny<Order>()))
                .ReturnsAsync((Order o) => new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerId = clienteInexistenteId,
                    ShippingAddress = "Calle Mitre 123",
                    BillingAddress = "Calle Mitre 123",
                    Notes = "Sin notas",
                    OrderItems = [new OrderItem {
                        ProductId = productsInDb[0].Id,
                        Quantity = 1
                }]
                });

        var service = new OrdersManagementService(repoMock.Object, loggerMock.Object);

        // 2. ACT (Ejecutar la unidad)
        var orderRequest = new OrderModel.RequestOrder(ordenNuevo.ShippingAddress, ordenNuevo.BillingAddress, ordenNuevo.Notes, ordenNuevo.CustomerId, [new OrderItemModel.RequestItem(productsInDb[0].Id, 1)]);

        // 3. ASSERT (Verificar resultados)

        var ex = await Assert.ThrowsAsync<ClientNotFoundException>(() => service.AddOrder(orderRequest));
        Assert.Equal($"No se encontro un cliente con ID {ordenNuevo.CustomerId}", ex.Message);
        repoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
    } 
}
