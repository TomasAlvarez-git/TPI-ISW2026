using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos;

public record OrderItemModel
{
    public record RequestItem(Guid ProductId, int Quantity);
    public record ResponseItem(Guid ProductId, string? Name, string? Description, decimal? UnitPrice, int? Quantity);
}
  