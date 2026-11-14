using System.Net;
using System.Net.Http.Json;
using Dsw2026Tpi.E2ETests.Helpers;
using Dsw2026Tpi.E2ETests.Models;

namespace Dsw2026Tpi.E2ETests.Flows;

[Trait("Category", "E2E")]
public class CreateOrderFlowTests : IAsyncLifetime
{
    private ApiClient _admin = null!;
    private ApiClient _customer = null!;

    public async Task InitializeAsync()
    {
        _admin = await ApiClient.CreateAuthenticatedAsync("ADMIN");
        _customer = await ApiClient.CreateAuthenticatedAsync("USER");
    }

    public Task DisposeAsync()
    {
        _admin.Dispose();
        _customer.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateOrder_WhenCustomerBuysAvailableProduct_DecreasesStockAndReturnsCreated()
    {
        // ARRANGE ─ TODO 1: crear el producto (como ADMIN)
            var sku = TestData.Sku();
            var internalCode = TestData.InternalCode();     

            var productRequest = new ProductRequest(
                sku,
                internalCode,
                "Teclado Mecanico",
                "Teclado RGB para pruebas",
                100.0m,
                10
            );

            var createProductResponse = await _admin.Http.PostAsJsonAsync("/api/products", productRequest);
            Assert.Equal(HttpStatusCode.Created, createProductResponse.StatusCode);

            var product = await createProductResponse.Content.ReadFromJsonAsync<ProductResponse>();
            Assert.NotNull(product);

        // ACT ─ TODO 2: listar productos

            var skuDigits = sku.Replace("SKU-", "");
            
            var listResponse = await _customer.Http.GetAsync($"/api/products?search={skuDigits}");
            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

            var page = await listResponse.Content.ReadFromJsonAsync<ProductPage>();
            Assert.NotNull(page);
            Assert.Contains(page.Products, p => p.Id == product.Id);

        //       TODO 3: crear la orden (como USER)

            var item = new OrderItemRequest(product.Id, 3);

            var orderRequest = new OrderRequest(
                "Calle falsa, Famailla",
                "Calle falsa, San Miguel de Tucuman",
                "Pedido de prueba E2E",
                _customer.Session!.CustomerId!.Value,
                new List<OrderItemRequest> { item });

            var orderResponse = await _customer.Http.PostAsJsonAsync("/api/orders", orderRequest);

        // ASSERT ─ TODO 4: verificar la orden

            Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);

            var body = await orderResponse.Content.ReadFromJsonAsync<OrderResponse>();
            Assert.NotNull(body);

            var orderItem = Assert.Single(body.OrderItems);
            Assert.Equal(3, orderItem.Quantity);
            Assert.Equal(product.Id, orderItem.ProductId);    

        //          TODO 5: verificar que bajó el stock
            var getProductResponse = await _customer.Http.GetAsync($"/api/products/{product.Id}");
            Assert.Equal(HttpStatusCode.OK, getProductResponse.StatusCode);

            var updatedProduct = await getProductResponse.Content.ReadFromJsonAsync<ProductResponse>();
            Assert.NotNull(updatedProduct);
            Assert.Equal(7, updatedProduct.StockQuantity);

    }
}