using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities;

public class OrderItem : EntityBase
{
    [NotMapped]
    public decimal? SubTotal => Quantity * UnitPrice;
    public Guid ProductId { get; set; }
    
    private Product? _product;
    public Product? Product
    {
        get => _product;
        set
        {
            if (Product?.IsActive == false)
            {
                throw new InvalidOperationException("El producto debe estar activo");
            }
            _product = value;
        }
    }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? UnitPrice { get; set; }
    public int Quantity { get; set; }

    public Guid OrderId { get; set; }
    public Order? Order { get; set; }

    public OrderItem(Guid productId, Product product, string name, string? description, decimal unitPrice, int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentNullException(nameof(productId), "El ID del producto no puede estar vacio");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "La cantidad debe ser mayor a 0");
        
        if (!product.StockControl(quantity))
            throw new System.ApplicationException($"No hay suficiente stock, el producto {product.Id} tiene {product.StockQuantity} items en existencia");
        ProductId = productId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? "";
        UnitPrice = unitPrice; 
        Quantity = quantity;
    }
    public OrderItem() { }

}
