namespace Dsw2025Tpi.E2ETest.Models;

public record RegisterRequest(string Username, string Password, string Email, string Role);
public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string UserName, Guid? CustomerId, string? CustomerEmail);

public record ProductRequest(string Sku, string InternalCode, string Name, string? Description,
    decimal CurrentUnitPrice, int StockQuantity);
public record ProductResponse(Guid Id, string? Sku, string? InternalCode, string? Name, string? Description,
    decimal? CurrentUnitPrice, int? StockQuantity, bool IsActive);
public record ProductPage(List<ProductResponse> Products, int TotalFiltered, int TotalCount);

public record OrderItemRequest(Guid ProductId, int Quantity);
public record OrderItemResponse(Guid ProductId, string? Name, string? Description,
    decimal? UnitPrice, int? Quantity);
public record OrderRequest(string ShippingAddress, string BillingAddress, string? Notes,
    Guid CustomerId, List<OrderItemRequest> OrderItems);
public record OrderResponse(Guid Id, DateTime Date, string? ShippingAddress, string? BillingAddress,
    string? Notes, Guid CustomerId, string? CustomerName, string? Status,
    List<OrderItemResponse> OrderItems, decimal? TotalAmount);