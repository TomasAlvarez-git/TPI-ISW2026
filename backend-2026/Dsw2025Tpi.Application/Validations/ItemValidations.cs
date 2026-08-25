using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Dsw2025Tpi.Application.Validations;

public class ItemValidations
{
    public static void ValidateItem(OrderItemModel.RequestItem item)
    {
        GeneralValidations.ValidateNotNull(item, nameof(item));
        GeneralValidations.ValidateGuid(item.ProductId.ToString(), nameof(item.ProductId));
        GeneralValidations.ValidatePositiveWholeNumber(item.Quantity.ToString(), nameof(item.Quantity));
    }
    public static async Task AddStock(List<OrderItem> orderItems, IRepository _repository)
    {
        foreach (var item in orderItems)
        {
            var product = await _repository.GetById<Product>(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                await _repository.Update(product);
            }
        }
    }
}