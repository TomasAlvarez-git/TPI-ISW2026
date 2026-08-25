using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Validations;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Dsw2025Tpi.Application.Services;

public class AuthenticateService : IAuthenticateService
{
    private readonly IConfiguration _config;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AuthenticateService> _logger;
    private readonly IRepository _repository;

    public AuthenticateService(
        IConfiguration config,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IRepository repository,
        ILogger<AuthenticateService> logger)
    {
        _config = config;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger; 
        _repository = repository;
    }

    public string GenerateToken(string userName, string role)
    {
        var jwtConfig = _config.GetSection("Jwt");
        var keyText = jwtConfig["Key"] ?? throw new ArgumentNullException("Jwt Key");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,userName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtConfig["Issuer"],
            audience: jwtConfig["Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(double.Parse(jwtConfig["ExpireInMinutes"] ?? "60")),
            signingCredentials: creds
         );
        _logger.LogInformation("Token generado");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<LoginModel.ResponseLogin> Login(LoginModel.RequestLogin request)
    {
        _logger.LogInformation("Solicitud de ingreso");
        var user = await _userManager.FindByNameAsync(request.Username);
        
        Validations.AuthenticateValidations.ValidateLogin(request, user!);

        var result = await _signInManager.CheckPasswordSignInAsync(user!, request.Password, false);
        if (!result.Succeeded)
        {
            throw new Application.Exceptions.NotAuthenticatedException("El nombre de usuario o contraseña son incorrectos");
        }

        var roles = await _userManager.GetRolesAsync(user!);

        var role = roles.FirstOrDefault() ?? throw new Application.Exceptions.InvalidRoleException("El usuario no tiene asignado un rol");

        var email = user!.Email ?? string.Empty;

        if (role == "USER")
        {
            var customer = await _repository.GetByEmail<Customer>(email);

            if (customer == null)
            {
                throw new Application.Exceptions.NotFoundException("El usuario no se encuentra asociado a un cliente");
            }
            
            var token = GenerateToken(user!.UserName!, role);

            _logger.LogInformation("Solicitud de ingreso de user exitosa");
            return new LoginModel.ResponseLogin(token, user.UserName!,customer!.Id, customer.Email!);

        }
        else
        {
            var tokenAdmin = GenerateToken(user!.UserName!, role);
            _logger.LogInformation("Solicitud de ingreso de admin exitosa");
            return new LoginModel.ResponseLogin(tokenAdmin, user.UserName!);

        }
    }
    public async Task<RegisterModel.ResponseRegister> Register(RegisterModel.RequestRegister model)
    {
        _logger.LogInformation("Solicitud de registro");
        AuthenticateValidations.ValidateRegistration(model, _userManager);
        var user = new IdentityUser { UserName = model.Username, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            throw new Application.Exceptions.ApplicationException(result.Errors.ToString());

        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);

        if (!roleResult.Succeeded)
            throw new Application.Exceptions.InvalidRoleException("Hubo un problame asignando el rol");

        if(model.Role == "USER")
        {
            var customer = new Customer(model.Email, model.Username, null);
            await _repository.Add(customer);
        }

        _logger.LogInformation("Solicitud de registro exitosa");
        return new RegisterModel.ResponseRegister(user.UserName, user.Email, model.Role);

    }
}
