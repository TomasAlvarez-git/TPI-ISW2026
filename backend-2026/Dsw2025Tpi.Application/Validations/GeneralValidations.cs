using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Dsw2025Tpi.Application.Validations;

public static class GeneralValidations
{
    public static void ValidateNotNull<T>(T obj, string paramName)
    {
        if(string.IsNullOrWhiteSpace(obj!.ToString())
            && obj is not IFormFile
            && obj is not IEnumerable<string>
            && obj is not IEnumerable<int>
            && obj is not IEnumerable<Guid>)
        {
            throw new Exceptions.ArgumentNullException($"{paramName} no puede estar vacio o en blanco");
        }
    }

    public static void ValidateText(string text, string paramName)
    {
        ValidateNotNull(text, paramName);
        if (!Regex.IsMatch(text, @"^[a-zA-Z0-9\s,.]*$"))
        {
            throw new Exceptions.NumbersAndLettersException($"{paramName}: input no valido, debe ser una cadena de letras y/o numeros");
        }
    }
    public static void ValidateOptionalText(string text, string paramName)
    {
        if (!Regex.IsMatch(text, @"^[a-zA-Z0-9\s,.\-°º#]*$"))
        {
            throw new Exceptions.NumbersSimbolsAndLettersException($"{paramName}: input no valido, debe ser una cadena de letras, numeros y/o ciertos simbolos (.\\-°º#)");
        }
        if(text.Length >= 100)
        {
            throw new Exceptions.TooLongException($"{paramName}: input no valido, demasiado largo");
        }

    }
    public static void ValidateNumber(string number, string paramName)
    {
        ValidateText(number, paramName);
        if(int.TryParse(number, out int numberInt) || (decimal.TryParse(number, out decimal numberDecimal)))
        {
            return;
        }
        else
        {
            throw new Application.Exceptions.NumbersException($"{paramName} no es valido, debe ser un numero");
        }
    }
    public static void ValidatePositiveWholeNumberAndCero(string number, string paramName)
    {
        ValidateNumber(number, paramName);
        if (!Regex.IsMatch(number, @"^(0|[1-9]\d*)$"))
        {
            throw new Exceptions.PositiveWholeNumberAndCeroException($"{paramName} input no valido, debe ser un numero entero o 0");
        }
    }
    public static void ValidatePositiveWholeNumber(string number, string paramName)
    {
        ValidateNumber(number, paramName);
        if (!Regex.IsMatch(number, @"^[1-9]\d*$"))
        {
            throw new Exceptions.PositiveWholeNumberException($"{paramName} input no valido, debe ser un numero entero positivo");
        }
    }

    public static void ValidatePositiveDecimalNumber(string number, string paramName)
    {
        ValidateNumber(number, paramName);
        if (!Regex.IsMatch(number, @"^[0-9][0-9,\.]*$"))
        {
            throw new Exceptions.PositiveDecimalNumberException($"{paramName} input no valido, debe ser un numero decimal positivo");
        }
    }
    public static void ValidateGuid(string text, string paramName)
    {
        ValidateNotNull(text, paramName);
        if (!Regex.IsMatch(text, @"^[a-zA-Z0-9-\-]*$"))
        {
            throw new Exceptions.InvalidGuidException($"{paramName}: guid no valido");
        }
    }
}
