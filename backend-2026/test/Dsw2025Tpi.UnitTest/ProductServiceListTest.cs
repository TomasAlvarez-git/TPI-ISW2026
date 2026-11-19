using System.Linq.Expressions;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dsw2025Tpi.UnitTest
{
    public class ProductServiceListTest
    {
        private static Mock<IRepository> CreateRepositoryMock(List<Product> productsInDb)
        {
            var repoMock = new Mock<IRepository>();

            repoMock
                .Setup(r => r.GetFiltered(
                    It.IsAny<Expression<Func<Product, bool>>>(),
                    It.IsAny<string[]>()))
                .ReturnsAsync((Expression<Func<Product, bool>> predicate, string[] _) =>
                    productsInDb.Where(predicate.Compile()).ToList());

            repoMock
                .Setup(r => r.GetAll<Product>(It.IsAny<string[]>()))
                .ReturnsAsync(productsInDb);

            return repoMock;
        }

        [Fact]
        public async Task GetProducts_WhenIncludeIsActiveIsTrue_ReturnsOnlyActiveProducts()
        {
            // 1. ARRANGE (Preparar dependencias y mocks)
            var productsInDb = new List<Product>
            {
                new Product
                {
                    Sku = "SKU-9567",
                    InternalCode = "INT-9567",
                    Name = "Auriculares con cancelacion de ruido",
                    Description = "Auriculares con cancelacion de ruido",
                    CurrentUnitPrice = 1200,
                    StockQuantity = 20,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-9569",
                    InternalCode = "INT-9569",
                    Name = "Auriculares sin cancelacion de ruido",
                    Description = "Auriculares sin cancelacion de ruido",
                    CurrentUnitPrice = 1200,
                    StockQuantity = 20,
                    IsActive = false
                }
            };

            var repoMock = CreateRepositoryMock(productsInDb);
            var loggerMock = new Mock<ILogger<ProductsManagementService>>();
            var service = new ProductsManagementService(repoMock.Object, loggerMock.Object);

            // 2. ACT (Ejecutar la unidad)
            var resultado = await service.GetAuthProducts(new ProductModel.FilterAuthProduct("active", null, null, null));

            // 3. ASSERT (Verificar resultados)
            Assert.NotNull(resultado);
            Assert.Single(resultado.Products);
            Assert.All(resultado.Products, p => Assert.True(p.IsActive));
            Assert.Equal("SKU-9567", resultado.Products[0].Sku);
            Assert.Equal(1, resultado.TotalFiltered);
            Assert.Equal(2, resultado.TotalCount);

            repoMock.Verify(r => r.GetFiltered<Product>(It.IsAny<Expression<Func<Product, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetProducts_WithPaginationFilter_AppliesSkipAndTakeCorrectly()
        {
            // 1. ARRANGE (Preparar dependencias y mocks)
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
                },
                new Product
                {
                    Sku = "SKU-0006",
                    InternalCode = "INT-0006",
                    Name = "Mouse inalámbrico no ergonómico",
                    Description = "Mouse óptico inalámbrico con receptor USB",
                    CurrentUnitPrice = 2200,
                    StockQuantity = 30,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0007",
                    InternalCode = "INT-0007",
                    Name = "Teclado mecánico retroiluminado",
                    Description = "Teclado mecánico con switches táctiles e iluminación RGB",
                    CurrentUnitPrice = 4500,
                    StockQuantity = 15,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0008",
                    InternalCode = "INT-0008",
                    Name = "Mouse inalámbrico ergonómico",
                    Description = "Mouse óptico inalámbrico con receptor USB y diseño ergonómico",
                    CurrentUnitPrice = 2200,
                    StockQuantity = 30,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0009",
                    InternalCode = "INT-0009",
                    Name = "Monitor LED 24 pulgadas",
                    Description = "Monitor Full HD de 24 pulgadas con panel IPS y entrada HDMI",
                    CurrentUnitPrice = 18500,
                    StockQuantity = 8,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0010",
                    InternalCode = "INT-0010",
                    Name = "Webcam Full HD 1080p",
                    Description = "Cámara web con micrófono integrado y enfoque automático",
                    CurrentUnitPrice = 3400,
                    StockQuantity = 12,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0011",
                    InternalCode = "INT-0011",
                    Name = "Soporte ajustable para laptop",
                    Description = "Base de aluminio plegable y regulable en altura para notebooks",
                    CurrentUnitPrice = 1800,
                    StockQuantity = 25,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0012",
                    InternalCode = "INT-0012",
                    Name = "Hub USB-C multipuerto",
                    Description = "Adaptador 5 en 1 con salidas HDMI, USB 3.0 y lector de tarjetas SD",
                    CurrentUnitPrice = 2900,
                    StockQuantity = 18,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0013",
                    InternalCode = "INT-0013",
                    Name = "Disco SSD externo 500GB",
                    Description = "Unidad de estado sólido portátil de alta velocidad con conexión USB 3.2",
                    CurrentUnitPrice = 9800,
                    StockQuantity = 10,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0014",
                    InternalCode = "INT-0014",
                    Name = "Micrófono condensador USB",
                    Description = "Micrófono cardioide de escritorio con trípode y filtro antipop",
                    CurrentUnitPrice = 5600,
                    StockQuantity = 14,
                    IsActive = true
                },
                new Product
                {
                    Sku = "SKU-0015",
                    InternalCode = "INT-0015",
                    Name = "Alfombrilla de mouse XL",
                    Description = "Mousepad extendido antideslizante con bordes cosidos",
                    CurrentUnitPrice = 1100,
                    StockQuantity = 40,
                    IsActive = true
                }
            };

            var repoMock = CreateRepositoryMock(productsInDb);
            var loggerMock = new Mock<ILogger<ProductsManagementService>>();
            var service = new ProductsManagementService(repoMock.Object, loggerMock.Object);

            // 2. ACT (Ejecutar la unidad)
            var resultado = await service.GetProducts(new ProductModel.FilterProduct(null, 2, 5));

            // 3. ASSERT (Verificar resultados)
            Assert.NotNull(resultado);
            Assert.Equal(5, resultado.Products.Count);
            Assert.Equal(15, resultado.TotalFiltered);
            Assert.Equal(15, resultado.TotalCount);

            var expectedSkus = new[] { "SKU-0006", "SKU-0007", "SKU-0008", "SKU-0009", "SKU-0010" };
            Assert.Equal(expectedSkus, resultado.Products.Select(p => p.Sku));

            repoMock.Verify(r => r.GetFiltered<Product>(It.IsAny<Expression<Func<Product, bool>>>()), Times.Once);
        }
    }
}
