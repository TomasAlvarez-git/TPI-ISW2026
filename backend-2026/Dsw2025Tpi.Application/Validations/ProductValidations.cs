using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace Dsw2025Tpi.Application.Validations;

public class ProductValidations
{
    public static void ValidateProduct(ProductModel.RequestProduct request)
    {
        GeneralValidations.ValidateNotNull(request, nameof(request));
        
        GeneralValidations.ValidateNotNull(request.Sku, nameof(request.Sku));
        if (!Regex.IsMatch(request.Sku, @"^SKU-\d{4}$"))
        {
            throw new Exceptions.InvalidSkuException("SKU ingresado es no valido, el formato valido es = 'SKU-XXXX'");
        }

        GeneralValidations.ValidateNotNull(request.InternalCode, nameof(request.InternalCode));
        if (!Regex.IsMatch(request.InternalCode, @"^INT-\d{4}$"))
        {
            throw new Exceptions.InvalidInternalCodeException("InternalCode ingresado es no valido,  el formato valido es = 'INT-XXXX'");
        }

        GeneralValidations.ValidateText(request.Name, nameof(request.Name));
        GeneralValidations.ValidatePositiveDecimalNumber(request.CurrentUnitPrice.ToString(), nameof(request.CurrentUnitPrice));
        if (request.CurrentUnitPrice <= 0)
            throw new Exceptions.PositivePriceException("El precio actual por unidad no puede ser negativo o 0");
        GeneralValidations.ValidatePositiveWholeNumberAndCero(request.StockQuantity.ToString(), nameof(request.StockQuantity));
        GeneralValidations.ValidateOptionalText(request.Description!, nameof(request.Description));
    }
    public async static Task ValidateAddedProduct(ProductModel.RequestProduct request, IRepository _repository)
    {
        ValidateProduct(request);
        if (await _repository.First<Product>(p => p.Sku == request.Sku) != null)
            throw new Exceptions.DuplicatedSkuException($"Un producto con este SKU ya existe {request.Sku}");
        if (await _repository.First<Product>(p => p.InternalCode == request.InternalCode) != null)
            throw new Exceptions.DuplicatedInternalCodeException($"Un producto con este codigo interno ya existe {request.InternalCode}");
    }

    public async static Task ValidateUpdatedProduct(Product product, IRepository _repository)
    {
        ValidateActiveProduct(product);
        if (await _repository.First<Product>(p => p.Sku == product.Sku && p.Id != product.Id) != null)
            throw new Exceptions.DuplicatedSkuException($"Ya existe otro producto diferente con este SKU {product.Sku}");
        if (await _repository.First<Product>(p => p.InternalCode == product.InternalCode && p.Id != product.Id) != null)
            throw new Exceptions.DuplicatedInternalCodeException($"Ya existe otro producto diferente con este codigo interno {product.InternalCode}");
    }

    public async static Task ValidateExistingProduct(Guid id, IRepository _repository)
    {
        GeneralValidations.ValidateGuid(id.ToString(), nameof(id));
        if (await _repository.First<Product>(p => p.Id == id) == null)
            throw new Exceptions.EntityNotFoundException($"No se encontro un producto con este ID {id}");
    }

    public static void ValidateActiveProduct(Product product)
    {
        GeneralValidations.ValidateNotNull(product, nameof(product));
        if(product.StockQuantity == 0)
        {
            product.IsActive = false;
        }
        if (!product.IsActive)
            throw new Exceptions.InactiveProductException($"El producto con ID {product.Id} no esta activo");
    }

    public static void ValidateFilteredArguments(ProductModel.FilterProduct request, IRepository _repository)
    {

        GeneralValidations.ValidateNotNull(request, nameof(request));

        if (!string.IsNullOrEmpty(request.Search))
        {
            GeneralValidations.ValidateText(request.Search!, nameof(request.Search));
        }

        if (!string.IsNullOrEmpty(request.PageNumber.ToString()))
        {
            GeneralValidations.ValidatePositiveWholeNumberAndCero(request.PageNumber.ToString()!, nameof(request.PageNumber));
            if (request.PageNumber > 1000)
                throw new Exceptions.PageNumberException("El numero de pagina no puede ser mayor a 1000");
        }
        if (request.PageSize is not null)
        {
            GeneralValidations.ValidatePositiveWholeNumberAndCero(request.PageSize.ToString()!, nameof(request.PageSize));
            if (request.PageSize > 20)
                throw new Exceptions.PageSizeException("El tamaño de la pagina no puede ser mayor a 20");
        }
    }

}


        
