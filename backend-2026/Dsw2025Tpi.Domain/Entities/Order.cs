using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities;

public class Order : EntityBase
{
    public DateTime Date { get; set; }
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? Notes { get; set; }
    public OrderStatus? Status { get; set; } 

    [NotMapped]
    public decimal? TotalAmount => OrderItems.Sum(i => i.Quantity * i.UnitPrice);

    public List<OrderItem> OrderItems { get; set; } = new();
    
    public Order(string shippingAddress, string billingAddress, string? notes, Guid customerId, List<OrderItem>? orderItems)
    {
        Date = DateTime.UtcNow;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        Notes = notes;
        CustomerId = customerId;
        OrderItems = orderItems ?? new List<OrderItem>();
        Status = OrderStatus.PENDING;
    }
    public Order() { }
    public void AddOrderItem(Guid productId, Product product, int quantity, string name, string? description, decimal unitPrice)
    {
        OrderItems.Add(new OrderItem(productId, product, name, description, unitPrice, quantity));
    }
}
