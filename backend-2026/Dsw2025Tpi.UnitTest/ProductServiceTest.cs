using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Moq;

using Microsoft.Extensions.Logging;

namespace Dsw2025Tpi.UnitTest
{
    public class ProductServiceTest
    {
        [Fact]
        public async Task CrearProducto_DatosValidos_DevuelveProductoCreado()
        {
            // 1. ARRANGE (Preparar dependencias y mocks)
            var repoMock = new Mock<IRepository>();
            var productoNuevo = new Product { Sku = "SKU-9567",
                InternalCode = "INT-9567", Name = "Auriculares con cancelacion de ruido", CurrentUnitPrice = 1200, StockQuantity = 20 };

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
            Assert.NotNull(resultado);
            Assert.NotEqual(Guid.Empty, resultado.Id);
            Assert.Equal(productRequest.Sku, resultado.Sku);
            Assert.Equal(productRequest.InternalCode, resultado.InternalCode);
            Assert.Equal(productRequest.Name, resultado.Name);
            Assert.Equal(productRequest.CurrentUnitPrice, resultado.CurrentUnitPrice);
            Assert.Equal(productRequest.Description, resultado.Description);
            Assert.Equal(productRequest.StockQuantity, resultado.StockQuantity);

            repoMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        }
    }
}
