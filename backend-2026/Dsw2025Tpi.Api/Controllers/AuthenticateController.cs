using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Dsw2025Tpi.Application.Validations;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticateController : ControllerBase
{
    private readonly IAuthenticateService _service;

    public AuthenticateController(IAuthenticateService service,
        SignInManager<IdentityUser> signInManager)
    {
        _service = service ?? throw new Application.Exceptions.ArgumentNullException(nameof(service));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel.RequestLogin request)
    {
        var token = await _service.Login(request);
        return Ok(token);
    }

    [HttpPost("register")]
    //[Authorize(Roles = "ADMIN")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterModel.RequestRegister model)
    {
        await _service.Register(model);
        return Ok("Nuevo usuario creado con exito");
    }
}
