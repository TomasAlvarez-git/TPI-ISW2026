using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

using Microsoft.Extensions.Logging;
using Azure;
using System.Linq.Expressions;
using System.Net;
using FluentAssertions;
using Dsw2025Tpi.Application.Exceptions;

namespace Dsw2025Tpi.UnitTest
{
    public class ProductServiceCreateTest
    {
        [Fact]
        public async Task AddProduct_WithValidData_ReturnsProductAndCallsRepositoryAdd()
        {
            // 1. ARRANGE (Preparar dependencias y mocks)
            var repoMock = new Mock<IRepository>();
            var productoNuevo = new Product { 
                Sku = "SKU-9567",
                InternalCode = "INT-9567", 
                Name = "Auriculares con cancelacion de ruido",
                Description = "Auriculares con cancelacion de ruido",
                CurrentUnitPrice = 1200, 
                StockQuantity = 20 
            };

            var productoIdEsperado = Guid.NewGuid();

            var loggerMock = new Mock<ILogger<ProductsManagementService>>();
            repoMock.Setup(r => r.Add(It.IsAny<Product>()))
                    .ReturnsAsync((Product p) => new Product
                    {
                        Id = Guid.NewGuid(),
                        Sku = p.Sku,
                        InternalCode = p.InternalCode,
                        Name = p.Name,
                        Description = p.Description,
                        CurrentUnitPrice = p.CurrentUnitPrice,
                        StockQuantity = p.StockQuantity,
                        IsActive = true
                    });

            var service = new ProductsManagementService(repoMock.Object, loggerMock.Object);

            // 2. ACT (Ejecutar la unidad)
            var productRequest = new ProductModel.RequestProduct(productoNuevo.Sku, productoNuevo.InternalCode, productoNuevo.Name,
                productoNuevo.Description, productoNuevo.CurrentUnitPrice, (int)productoNuevo.StockQuantity);
            var resultado = await service.AddProduct(productRequest);

            // 3. ASSERT (Verificar resultados)
            Assert.Null(resultado);
            Assert.NotEqual(Guid.Empty, resultado.Id);
            Assert.Equal(productRequest.Sku, resultado.Sku);
            Assert.Equal(productRequest.InternalCode, resultado.InternalCode);
            Assert.Equal(productRequest.Name, resultado.Name);
            Assert.Equal(productRequest.CurrentUnitPrice, resultado.CurrentUnitPrice);
            Assert.Equal(productRequest.Description, resultado.Description);
            Assert.Equal(productRequest.StockQuantity, resultado.StockQuantity);

            repoMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task AddProduct_WithDuplicateSku_ThrowsDuplicatedEntityException()
        {
            // 1. ARRANGE (Preparar dependencias y mocks)
            var repoMock = new Mock<IRepository>();
            var productoNuevo = new Product
            {
                Sku = "SKU-9567",
                InternalCode = "INT-9568",
                Name = "Auriculares con cancelacion de ruido",
                Description = "Auriculares con cancelacion de ruido",
                CurrentUnitPrice = 1200,
                StockQuantity = 20
            };

            var productoIdEsperado = Guid.NewGuid();

            var loggerMock = new Mock<ILogger<ProductsManagementService>>();
            repoMock.Setup(r => r.First(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<string[]>()))
                    .ReturnsAsync(new Product
                    {
                        Id = productoIdEsperado,
                        Sku = "SKU-9567",
                        InternalCode = "INT-0001",
                        IsActive = true
                    });

            var service = new ProductsManagementService(repoMock.Object, loggerMock.Object);

            // 2. ACT & 3. ASSERT (Verificar resultados)
            var productRequest = new ProductModel.RequestProduct(productoNuevo.Sku, productoNuevo.InternalCode, productoNuevo.Name,
                productoNuevo.Description, productoNuevo.CurrentUnitPrice, (int)productoNuevo.StockQuantity);

            var ex = await Assert.ThrowsAsync<DuplicatedSkuException>(() => service.AddProduct(productRequest));
            Assert.Equal("Un producto con este SKU ya existe SKU-9567", ex.Message);
            repoMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
        }
        [Fact]
        public async Task AddProduct_WithInvalidStock_ThrowsInvalidDataException()
        {
            // 1. ARRANGE (Preparar dependencias y mocks)
            var repoMock = new Mock<IRepository>();
            var productoNuevo = new Product
            {
                Sku = "SKU-9568",
                InternalCode = "INT-9568",
                Name = "Auriculares con cancelacion de ruido",
                Description = "Auriculares con cancelacion de ruido",
                CurrentUnitPrice = 1200,
                StockQuantity = -5
            };

            var loggerMock = new Mock<ILogger<ProductsManagementService>>();
            var service = new ProductsManagementService(repoMock.Object, loggerMock.Object);

            // 2. ACT & 3. ASSERT (Verificar resultados)
            var productRequest = new ProductModel.RequestProduct(productoNuevo.Sku, productoNuevo.InternalCode, productoNuevo.Name,
                productoNuevo.Description, productoNuevo.CurrentUnitPrice, (int)productoNuevo.StockQuantity);

            var ex = await Assert.ThrowsAsync<NumbersAndLettersException>(() => service.AddProduct(productRequest));
            Assert.Equal("StockQuantity: input no valido, debe ser una cadena de letras y/o numeros", ex.Message);
            repoMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task AddProduct_WithInvalidPrice_ThrowsInvalidDataException()
        {
            // 1. ARRANGE (Preparar dependencias y mocks)
            var repoMock = new Mock<IRepository>();
            var productoNuevo = new Product
            {
                Sku = "SKU-9568",
                InternalCode = "INT-9568",
                Name = "Auriculares con cancelacion de ruido",
                Description = "Auriculares con cancelacion de ruido",
                CurrentUnitPrice = -1200,
                StockQuantity = 50
            };

            var loggerMock = new Mock<ILogger<ProductsManagementService>>();
            var service = new ProductsManagementService(repoMock.Object, loggerMock.Object);

            // 2. ACT & 3. ASSERT (Verificar resultados)
            var productRequest = new ProductModel.RequestProduct(productoNuevo.Sku, productoNuevo.InternalCode, productoNuevo.Name,
                productoNuevo.Description, productoNuevo.CurrentUnitPrice, (int)productoNuevo.StockQuantity);

            var ex = await Assert.ThrowsAsync<NumbersAndLettersException>(() => service.AddProduct(productRequest));
            Assert.Equal("CurrentUnitPrice: input no valido, debe ser una cadena de letras y/o numeros", ex.Message);
            repoMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
        }
    }
}
