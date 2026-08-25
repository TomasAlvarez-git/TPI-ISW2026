using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos;

public record RegisterModel
{
    public record RequestRegister(string Username, string Password, string Email, string Role);
    public record ResponseRegister(string Username, string Email, string Role);
}

