using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Dsw2025Tpi.Application.Dtos
{
    public record LoginModel
    {
        public record RequestLogin(string Username, string Password);
        public record ResponseLogin(string Token, string UserName, Guid? CustomerId = null, string? CustomerEmail = null);
    }
}
