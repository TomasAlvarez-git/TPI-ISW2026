using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Entities;
using static Dsw2025Tpi.Application.Dtos.OrderItemModel;
using static Dsw2025Tpi.Application.Dtos.ProductModel;

namespace Dsw2025Tpi.Application.Dtos;

public record OrderModel
{
    public record RequestChangeStatus(string NewStatus);
    public record RequestOrder(string ShippingAddress, string BillingAddress, string? Notes, Guid CustomerId, 
        List<RequestItem> OrderItems);
    public record ResponseOrder(Guid Id, DateTime Date, string? ShippingAddress, string? BillingAddress,
        string? Notes, Guid CustomerId, string? CustomerName, string? Status, List<ResponseItem> OrderItems, decimal? TotalAmount);

    public record ResponsePagination(List<ResponseOrder> Orders, int TotalFiltered, int TotalCount);
    public record FilterOrder(string? Status, string? CustomerName, int? PageNumber, int? PageSize);
    //No deberia ser nulleable los parametros del filter, principalmente el PageNumber y el pageSize
    //tiene que tener valor por defecto. Debido a que la validacion esta antes
}
