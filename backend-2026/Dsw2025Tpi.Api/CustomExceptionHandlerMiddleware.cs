using System.Net;
using System.Text.Json;
using Dsw2025Tpi.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api;
public class CustomExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<CustomExceptionHandlingMiddleware> _logger;
    public CustomExceptionHandlingMiddleware(ILogger<CustomExceptionHandlingMiddleware> logger)
    {
        _logger = logger ?? throw new Application.Exceptions.ArgumentNullException(nameof(logger));
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            context.Response.ContentType = "application/json";


            var statusCode = e switch
            {
                Application.Exceptions.EntityNotFoundException => HttpStatusCode.NotFound,
                Application.Exceptions.ArgumentOutOfRangeException => HttpStatusCode.BadRequest,
                Application.Exceptions.ArgumentNullException => HttpStatusCode.BadRequest,
                Application.Exceptions.ArgumentException => HttpStatusCode.BadRequest,
                Application.Exceptions.DuplicatedEntityException => HttpStatusCode.BadRequest,
                Application.Exceptions.InvalidStatusException => HttpStatusCode.BadRequest,
                Application.Exceptions.NotAuthenticatedException => HttpStatusCode.BadRequest,
                Application.Exceptions.InvalidRoleException => HttpStatusCode.BadRequest,
                Application.Exceptions.InvalidEmailException => HttpStatusCode.BadRequest,
                Application.Exceptions.ShortUsernameException => HttpStatusCode.BadRequest,
                Application.Exceptions.DuplicatedEmailException => HttpStatusCode.BadRequest,
                Application.Exceptions.InvalidPasswordException => HttpStatusCode.BadRequest,
                Application.Exceptions.DuplicatedUsernameException => HttpStatusCode.BadRequest,
                Application.Exceptions.NumbersAndLettersException => HttpStatusCode.BadRequest,
                Application.Exceptions.NumbersSimbolsAndLettersException => HttpStatusCode.BadRequest,
                Application.Exceptions.TooLongException => HttpStatusCode.BadRequest,
                Application.Exceptions.PositiveWholeNumberAndCeroException => HttpStatusCode.BadRequest,
                Application.Exceptions.PositiveWholeNumberException => HttpStatusCode.BadRequest,
                Application.Exceptions.PositiveDecimalNumberException => HttpStatusCode.BadRequest,
                Application.Exceptions.InvalidGuidException => HttpStatusCode.BadRequest,
                Application.Exceptions.PageNumberException => HttpStatusCode.BadRequest,
                Application.Exceptions.PageSizeException => HttpStatusCode.BadRequest,
                Application.Exceptions.OrderNotFoundException => HttpStatusCode.NotFound,
                Application.Exceptions.DuplicatedStatusException => HttpStatusCode.BadRequest,
                Application.Exceptions.CancelledOrderException => HttpStatusCode.BadRequest,
                Application.Exceptions.InvalidSkuException => HttpStatusCode.BadRequest,
                Application.Exceptions.InvalidInternalCodeException => HttpStatusCode.BadRequest,
                Application.Exceptions.PositivePriceException => HttpStatusCode.BadRequest,
                Application.Exceptions.DuplicatedSkuException => HttpStatusCode.BadRequest,
                Application.Exceptions.DuplicatedInternalCodeException => HttpStatusCode.BadRequest,
                Application.Exceptions.InactiveProductException => HttpStatusCode.BadRequest,
                Application.Exceptions.NumbersException => HttpStatusCode.BadRequest,
                Application.Exceptions.NotFoundException => HttpStatusCode.NotFound,
                Application.Exceptions.ClientNotFoundException => HttpStatusCode.NotFound,
                //Application.Exceptions.NoContentException => HttpStatusCode.NoContent,
                Application.Exceptions.NoContentException => HttpStatusCode.BadRequest,
                Application.Exceptions.NoOrdersForClientException => HttpStatusCode.BadRequest,
                Application.Exceptions.NoOrdersForStatusException => HttpStatusCode.BadRequest,
                InvalidOperationException => HttpStatusCode.BadRequest,
                Dsw2025Tpi.Application.Exceptions.ApplicationException => HttpStatusCode.BadRequest,
                System.ApplicationException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };

            var internalErrorCode = e switch
            {
                Application.Exceptions.EntityNotFoundException => "01",
                Application.Exceptions.ArgumentOutOfRangeException => "02",
                Application.Exceptions.ArgumentNullException => "03",
                Application.Exceptions.ArgumentException => "04",
                Application.Exceptions.DuplicatedEntityException => "05",
                Application.Exceptions.InvalidStatusException => "06",
                Application.Exceptions.InvalidRoleException => "12",
                Application.Exceptions.InvalidEmailException => "13",
                Application.Exceptions.ShortUsernameException => "14",
                Application.Exceptions.DuplicatedEmailException => "15",
                Application.Exceptions.InvalidPasswordException => "16",
                Application.Exceptions.DuplicatedUsernameException => "17",
                Application.Exceptions.NumbersAndLettersException => "18",
                Application.Exceptions.NumbersSimbolsAndLettersException => "19",
                Application.Exceptions.TooLongException => "20",
                Application.Exceptions.PositiveWholeNumberAndCeroException => "21",
                Application.Exceptions.PositiveWholeNumberException => "22",
                Application.Exceptions.PositiveDecimalNumberException => "23",
                Application.Exceptions.InvalidGuidException => "24",
                Application.Exceptions.PageNumberException => "25",
                Application.Exceptions.PageSizeException => "26",
                Application.Exceptions.NoOrdersForClientException => "27",
                Application.Exceptions.NoOrdersForStatusException => "28",
                Application.Exceptions.OrderNotFoundException => "29",
                Application.Exceptions.DuplicatedStatusException => "30",
                Application.Exceptions.CancelledOrderException => "31",
                Application.Exceptions.ClientNotFoundException => "32",
                Application.Exceptions.InvalidSkuException => "33",
                Application.Exceptions.InvalidInternalCodeException => "34",
                Application.Exceptions.PositivePriceException => "35",
                Application.Exceptions.DuplicatedSkuException => "36",
                Application.Exceptions.DuplicatedInternalCodeException => "37",
                Application.Exceptions.InactiveProductException => "38",
                Application.Exceptions.NumbersException => "39",
                Application.Exceptions.NotAuthenticatedException => "07",
                Application.Exceptions.NotFoundException => "08",
                Application.Exceptions.NoContentException => "09",
                InvalidOperationException => "10",
                Application.Exceptions.ApplicationException => "11",
                System.ApplicationException => "99",
                _ => "00"
            };

            var path = context.Request.Path.Value?.ToLower() ?? "";
            string source;
            if (path.Contains("orders"))
                source = "order";
            else if (path.Contains("products"))
                source = "product";
            else if (path.Contains("auth"))
                source = "auth";
            else
                source = "app";

            internalErrorCode = $"{source}-{internalErrorCode}";
            
            context.Response.StatusCode = (int)statusCode;

            var errorResponse = new
            {
                status = (int)statusCode,
                title = statusCode.ToString(),
                detail = e.Message,
                code = internalErrorCode
            };

            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }
}