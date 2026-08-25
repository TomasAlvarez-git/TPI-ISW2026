using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : Controller
{
    private readonly IOrdersManagementService _service;

    public OrderController(IOrdersManagementService service)
    {
        _service = service;
    }

    [HttpGet()]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<IActionResult> GetOrders([FromQuery] OrderModel.FilterOrder request)
    {
        var orders = await _service.GetOrders(request);
        if (orders == null)
        {
            Response.Headers.Append("X-Message", "No se tienen ordenes");
            return NoContent();
        }
        return Ok(orders);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "ADMIN,USER")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _service.GetOrderById(id);
        if (order == null)
            return NotFound();
        return Ok(order);
    }

    [HttpPost()]
    [Authorize(Roles = "USER")]
    public async Task<IActionResult> AddOrder([FromBody] OrderModel.RequestOrder request)
    {
        var order = await _service.AddOrder(request);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ChangeOrderStatusAsync(Guid id, [FromBody] OrderModel.RequestChangeStatus status)
    {
        var changedOrder = await _service.ChangeOrderStatus(id, status);
        return Ok(changedOrder);
    }
}