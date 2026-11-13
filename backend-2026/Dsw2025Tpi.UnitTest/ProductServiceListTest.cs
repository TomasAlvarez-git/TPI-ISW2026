using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

using Microsoft.Extensions.Logging;
using Azure;
using System.Net;
using FluentAssertions;
using Dsw2025Tpi.Application.Exceptions;

namespace Dsw2025Tpi.UnitTest
{
    public class ProductServiceListTest
    {
        [Fact]
        public async Task GetProducts_WhenIncludeInactiveIsFalse_ReturnsOnlyActiveProducts()
        {
            // 1. ARRANGE (Preparar dependencias y mocks)
            var repoMock = new Mock<IRepository>();

            var loggerMock = new Mock<ILogger<ProductsManagementService>>();

            repoMock.Setup(r => r.GetFiltered<Product>);

            var service = new ProductsManagementService(repoMock.Object, loggerMock.Object);

            // 2. ACT (Ejecutar la unidad)
            var resultado = await service.GetProducts(new ProductModel.FilterProduct(null, null, null));

            // 3. ASSERT (Verificar resultados)
            Assert.NotNull(resultado);

            repoMock.Verify(r => r.GetFiltered<Product>(It.IsAny<Func<Product, bool>>()), Times.Once);
        }

    }
}
