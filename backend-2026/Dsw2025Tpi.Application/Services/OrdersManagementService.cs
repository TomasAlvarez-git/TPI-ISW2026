using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Application.Validations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Dsw2025Tpi.Application.Dtos.OrderItemModel;
using Microsoft.Extensions.Logging;
using System.Collections;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace Dsw2025Tpi.Application.Services;

public class OrdersManagementService : IOrdersManagementService
{
    IRepository _repository;
    private readonly ILogger<OrdersManagementService> _logger;
    public OrdersManagementService(IRepository repository,
         ILogger<OrdersManagementService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<OrderModel.ResponsePagination>? GetOrders(OrderModel.FilterOrder request)
    {
        Customer client = null;
        if (request.Status is null && request.CustomerName is null && request.PageSize is null && request.PageNumber is null)
        {
            _logger.LogInformation("Consulta de ordenes sin filtrar");
        }
        else
        {
            _logger.LogInformation("Consulta de ordenes filtradas");
            Validations.OrderValidations.ValidateFilteredArguments(request, _repository);
        }

        OrderStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
            status = Enum.Parse<OrderStatus>(request.Status.ToUpper(), true);

        var filteredOrders = await _repository
        .GetFiltered<Order>(
            o =>
                o.Status!.Value != OrderStatus.CANCELLED
                && (request.CustomerName.IsNullOrEmpty() || o.Customer!.Name.Contains(request!.CustomerName))
                && (!status.HasValue || o.Status == status.Value)
                ,
            include: new[] { "OrderItems", "Customer" } 
        );
        OrderValidations.ValidateNotNullOrders(filteredOrders, request);
        int totalFiltered = filteredOrders!.Count();

        var allOrders = await _repository.GetAll<Order>();
        int totalCount = allOrders.Count(o => o.Status!.Value != OrderStatus.CANCELLED);

        var pagedOrders = filteredOrders
            .OrderByDescending(o => o.Date)
            .Skip((request.PageNumber - 1) * request.PageSize ?? 0)
            .Take(request.PageSize ?? filteredOrders.Count())
            .ToList();

        var responseOrders = pagedOrders.Select(order => new OrderModel.ResponseOrder(
        order.Id,
        order.Date,
        order.ShippingAddress,
        order.BillingAddress,
        order.Notes,
        order.CustomerId,
        order.Customer.Name,
        order.Status.ToString(),
        order.OrderItems.Select(i => new OrderItemModel.ResponseItem(
            i.ProductId, i.Name, i.Description, i.UnitPrice, i.Quantity)).ToList(),
        order.TotalAmount));

        return new OrderModel.ResponsePagination(responseOrders!.ToList(), totalFiltered, totalCount);
    }
    public async Task<OrderModel.ResponseOrder?> GetOrderById(Guid id)
    {
        Customer client = null;
        _logger.LogInformation("Consulta de orden por Id: {id}", id);
        await OrderValidations.ValidateExistingOrder(id, _repository);
        var order = await _repository.GetById<Order>(id, include: new[] { "OrderItems" });
        OrderValidations.ValidateCancelledOrder(order!);
        client = await _repository.GetById<Customer>(order.CustomerId);
        if (client == null)
        {
            throw new Application.Exceptions.NotFoundException("Cliente no encontrado");
        }
        order.Customer = client;
        return order != null ?
            new OrderModel.ResponseOrder(
                order.Id,
                order.Date,
                order.ShippingAddress,
                order.BillingAddress,
                order.Notes,
                order.CustomerId,
                order.Customer.Name,
                order.Status?.ToString(),
                order.OrderItems.Select(i => new OrderItemModel.ResponseItem(
                    i.ProductId, i.Name, i.Description, i.UnitPrice, i.Quantity)).ToList(),
                order.TotalAmount) :
            null;
    }
    public async Task<OrderModel.ResponseOrder> AddOrder(OrderModel.RequestOrder request)
    {
        _logger.LogInformation("Creacion de orden");
        _logger.LogInformation("REQUEST RECIBIDO: " + JsonSerializer.Serialize(request));

        GeneralValidations.ValidateNotNull(request, nameof(request));
        var items = request!.OrderItems!.Select(item => new RequestItem(
            item.ProductId,
            item.Quantity
        )).ToList();

        List<OrderItem> orderItems = new List<OrderItem>();

        foreach (var item in items)
        {
            var product = await _repository.GetById<Product>(item.ProductId);
            await ProductValidations.ValidateExistingProduct(item.ProductId, _repository);
            ProductValidations.ValidateActiveProduct(product!);

            ItemValidations.ValidateItem(item);
            OrderItem orderItem= new OrderItem(
                item.ProductId,
                product!,
                product!.Name!,
                product.Description,
                product.CurrentUnitPrice,
                item.Quantity
            );
            orderItem.Product = product;
            
            orderItems.Add(orderItem);
        }

        OrderValidations.ValidateOrder(request);
        OrderValidations.ValidateCustomer(request.CustomerId, _repository);
        
        var order = new Order(request.ShippingAddress, request.BillingAddress, request.Notes!, request.CustomerId, orderItems);
        Customer client = await _repository.GetById<Customer>(order.CustomerId);
        if (client == null)
        {
            throw new Application.Exceptions.NotFoundException("Cliente no encontrado");
        }
        order.Customer = client;
        await _repository.Add(order);
        _logger.LogInformation("Creacion de orden exitosa");
        return new OrderModel.ResponseOrder(
            order.Id,
            order.Date,
            order.ShippingAddress,
            order.BillingAddress,
            order.Notes,
            order.CustomerId,
            order.Customer.Name,
            order.Status?.ToString(),
            order.OrderItems.Select(i => new OrderItemModel.ResponseItem(
                i.ProductId, i.Name, i.Description, i.UnitPrice, i.Quantity)).ToList(),
            order.TotalAmount
        );
    }

    public async Task<OrderModel.ResponseOrder?> ChangeOrderStatus(Guid id, OrderModel.RequestChangeStatus request)
    {
        _logger.LogInformation("Cambiar estado de orden con Id: {id}", id);
        var order = await _repository.First<Order>(p => p.Id == id, include: new[] { "OrderItems" });
        await OrderValidations.ValidateExistingOrder(id, _repository);
        OrderValidations.ValidateOrderStatus(order!, request.NewStatus);

        order!.Status = Enum.Parse<OrderStatus>(request.NewStatus.ToUpper(), true);
        var updated = await _repository.Update(order);
        
        if(updated.Status == OrderStatus.CANCELLED)
        {
            await ItemValidations.AddStock(updated.OrderItems, _repository);
        }

        Customer client = await _repository.GetById<Customer>(order.CustomerId);
        if (client == null)
        {
            throw new Application.Exceptions.NotFoundException("Cliente no encontrado");
        }
        updated.Customer = client;

        _logger.LogInformation("Modificacion de orden exitosa");
        return new OrderModel.ResponseOrder(
            updated.Id,
            updated.Date,
            updated.ShippingAddress,
            updated.BillingAddress,
            updated.Notes,
            updated.CustomerId,
            updated.Customer.Name,
            updated.Status?.ToString(),
            updated.OrderItems.Select(i => new OrderItemModel.ResponseItem(
                i.ProductId, i.Name, i.Description, i.UnitPrice, i.Quantity)).ToList(),
            updated.TotalAmount
            );
    }
}

