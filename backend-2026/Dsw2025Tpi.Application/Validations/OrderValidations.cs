using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Dsw2025Tpi.Application.Validations;

public class OrderValidations
{
    public static void ValidateFilteredArguments(OrderModel.FilterOrder request, IRepository _repository)
    {
        GeneralValidations.ValidateNotNull(request, nameof(request));
        if(request.CustomerName is not null)
        {
            GeneralValidations.ValidateText(request.CustomerName, nameof(request.CustomerName));
            string customerName = request.CustomerName;
            var customerId = _repository
                .First<Customer>(c => c.Name == customerName || c.Name.Contains(customerName))
                .Result?.Id ?? throw new Exceptions.ClientNotFoundException($"No se encontro un cliente con nombre {customerName}");
            ValidateCustomer(customerId, _repository);
        }

        if (request.Status != null)
        {
            ValidateFilteredStatus(request.Status);
        }

        if (!string.IsNullOrEmpty(request.PageNumber.ToString()))
        {
            GeneralValidations.ValidatePositiveWholeNumberAndCero(request.PageNumber.ToString()!, nameof(request.PageNumber));
            if(request.PageNumber > 1000)
                throw new Exceptions.PageNumberException("El numero de pagina no puede ser mayor a 1000");
        }
        if (request.PageSize is not null)
        {
            GeneralValidations.ValidatePositiveWholeNumberAndCero(request.PageSize.ToString()!, nameof(request.PageSize));
            if (request.PageSize > 20)
                throw new Exceptions.PageSizeException("El tamaño de la pagina no puede ser mayor a 20");
        }
    }
    public static void ValidateNotNullOrders(IEnumerable<Order>? orders, OrderModel.FilterOrder? request)
    {
        GeneralValidations.ValidateNotNull(request, nameof(request));
        if (orders == null || !orders.Any()  && request != null && !request.CustomerName.IsNullOrEmpty())
            throw new Exceptions.NoOrdersForClientException($"No se encontraron ordenes del cliente {request!.CustomerName}");
        if (orders == null || !orders.Any() && request != null && !request.Status.IsNullOrEmpty())
            throw new Exceptions.NoOrdersForStatusException($"No se encontraron con estado {request!.Status}");
        if (orders == null || !orders.Any())
            throw new Exceptions.NoContentException("No se encontraron ordenes");

    }
    public static void ValidateOrder(OrderModel.RequestOrder request)
    {
        GeneralValidations.ValidateNotNull(request, nameof(request));
        GeneralValidations.ValidateNotNull(request.OrderItems, nameof(request.OrderItems));
        GeneralValidations.ValidateText(request.ShippingAddress!, nameof(request.ShippingAddress));
        GeneralValidations.ValidateText(request.BillingAddress!, nameof(request.BillingAddress));
        GeneralValidations.ValidateOptionalText(request.Notes!, nameof(request.Notes));
        GeneralValidations.ValidateGuid(request.CustomerId.ToString(), nameof(request.CustomerId));
    }

    public async static Task ValidateExistingOrder(Guid id, IRepository _repository)
    {
        GeneralValidations.ValidateGuid(id.ToString(), nameof(id));
        if (await _repository.First<Order>(p => p.Id == id) == null)
            throw new Exceptions.OrderNotFoundException($"Orden con ID {id} no encontrada");
    }

    public static void ValidateOrderStatus(Order order, string status)
    {
        GeneralValidations.ValidateNotNull(order, nameof(order));
        GeneralValidations.ValidateText(status, nameof(status));

        var statusUpper = status.ToUpper();

        if (order.Status.ToString() == statusUpper)
            throw new Exceptions.DuplicatedStatusException($"Orden ya se encuentra en estado {status}");

        var validStatuses = Enum.GetNames(typeof(OrderStatus))
            .Select(s => s.ToLowerInvariant());
        if (!validStatuses.Contains(status.ToLower()))
            throw new Exceptions.InvalidStatusException($"Estado {status} no valido. Los estados validos son: {string.Join(", ", validStatuses)}");

        if (order.Status == OrderStatus.PENDING && statusUpper != "PROCESSING" && statusUpper != "CANCELLED")
            throw new Exceptions.InvalidStatusException($"El estado de la orden es PENDING, no puede ser cambiado a: {status}");
        if (order.Status == OrderStatus.PROCESSING && statusUpper != "SHIPPED" && statusUpper != "CANCELLED")
            throw new Exceptions.InvalidStatusException($"El estado de la orden es PROCESSING, no puede ser cambiado a: {status}");
        if (order.Status == OrderStatus.SHIPPED && statusUpper != "DELIVERED" && statusUpper != "CANCELLED")
            throw new Exceptions.InvalidStatusException($"El estado de la orden es SHIPPED, no puede ser cambiado a: {status}");
        if(order.Status == OrderStatus.CANCELLED)
            throw new Exceptions.InvalidStatusException($"El estado de la orden es CANCELLED, no puede ser cambiado a: {status}");
    }

    public static void ValidateFilteredStatus(string status)
    {
        GeneralValidations.ValidateText(status, nameof(status));
        var validStatuses = Enum.GetNames(typeof(OrderStatus))
            .Select(s => s.ToLowerInvariant());
        if (!validStatuses.Contains(status.ToLower()))
            throw new Exceptions.InvalidStatusException($"Estado {status} no valido. Los estados validos son: {string.Join(", ", validStatuses)}");
    }
    public static void ValidateCancelledOrder(Order order)
    {
        if (order.Status.ToString() == "CANCELLED")
            throw new Exceptions.CancelledOrderException($"Order con ID {order.Id} esta cancelada");
    }

    public static void ValidateCustomer(Guid customerId, IRepository _repository)
    {
        GeneralValidations.ValidateGuid(customerId.ToString(), nameof(customerId));
        var customer = _repository.First<Customer>(c => c.Id == customerId).Result;
        if (customer == null)
            throw new Exceptions.ClientNotFoundException($"No se encontro un cliente con ID {customerId}");
    }
}
