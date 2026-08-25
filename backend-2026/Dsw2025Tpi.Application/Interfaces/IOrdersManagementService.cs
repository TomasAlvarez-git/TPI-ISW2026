using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Interfaces
{
    public interface IOrdersManagementService
    {
        Task<OrderModel.ResponseOrder> AddOrder(OrderModel.RequestOrder request);
        Task<OrderModel.ResponseOrder?> ChangeOrderStatus(Guid id, OrderModel.RequestChangeStatus request);
        Task<OrderModel.ResponseOrder?> GetOrderById(Guid id);
        Task<OrderModel.ResponsePagination>? GetOrders(OrderModel.FilterOrder request);
    }
}