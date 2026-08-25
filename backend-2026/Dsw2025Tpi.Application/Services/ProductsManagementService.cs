using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Application.Validations;
using Microsoft.Extensions.Logging;
using System.Net;
using Dsw2025Tpi.Application.Exceptions;


namespace Dsw2025Tpi.Application.Services;

public class ProductsManagementService : IProductsManagementService
{
    IRepository _repository;
    private readonly ILogger<ProductsManagementService> _logger;
    public ProductsManagementService(IRepository repository,
        ILogger<ProductsManagementService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task<ProductModel.ResponseProduct?> GetProductById(Guid id)
    {
        _logger.LogInformation("Consulta de producto por Id: {id}", id);
        await ProductValidations.ValidateExistingProduct(id, _repository);
        var product = await _repository.GetById<Product>(id);
        ProductValidations.ValidateActiveProduct(product!);

        return product != null ?
            new ProductModel.ResponseProduct(product.Id, product.Sku, product.InternalCode, product.Name, product.Description,
                product.CurrentUnitPrice, product.StockQuantity, product.IsActive) :
            null;
    }

    public async Task<ProductModel.ResponsePagination?> GetProducts(ProductModel.FilterProduct request)
    {
        //usuario normal no puede ver porductos inactivos
        _logger.LogInformation("Consulta de productos");

        if (request.Search is null && request.PageSize is null && request.PageNumber is null)
        {
            _logger.LogInformation("Consulta de productos sin filtrar");
        }
        else
        {
            _logger.LogInformation("Consulta de productos filtrados");
            Validations.ProductValidations.ValidateFilteredArguments(request, _repository);
        }

        var filterredProducts = await _repository.GetFiltered<Product>(p =>
            (
                p.IsActive
                && (string.IsNullOrEmpty(request.Search) || p.Name!.Contains(request.Search!) || p.Sku!.Contains(request.Search!)))
            );
        
        //if (filterredProducts is null || !filterredProducts.Any())
        //    throw new NoContentException("No se encontraron productos");
        var allProducts = await _repository.GetAll<Product>();

        var products = filterredProducts!.Select(p => new ProductModel.ResponseProduct(
                p.Id,
                p.Sku,
                p.InternalCode,
                p.Name,
                p.Description,
                p.CurrentUnitPrice,
                p.StockQuantity,
                p.IsActive))
            .OrderBy(p => p.Sku)
            .Skip(((request.PageNumber??1) - 1) * request.PageSize ?? 0)
            .Take(request.PageSize ?? filterredProducts!.Count());

        return new ProductModel.ResponsePagination(products.ToList(), filterredProducts!.Count(), allProducts!.Count());
    }
    public async Task<ProductModel.ResponsePagination?> GetAuthProducts(ProductModel.FilterAuthProduct request)
    {
        var isActive = request.Status == "active"
            ? (bool?)true
            : request.Status == "inactive"
                ? (bool?)false
                : null;

        _logger.LogInformation("Consulta de productos");
        var filterredProducts = await _repository.GetFiltered<Product>(p =>
             (
                 (isActive == null || p.IsActive == isActive)
                 &&
                 (
                     string.IsNullOrEmpty(request.Search) ||
                     p.Name!.Contains(request.Search!) ||
                     p.Sku!.Contains(request.Search!) ||
                     p.InternalCode!.Contains(request.Search!)
                 )
             )
         );

        //if (filterredProducts is null || !filterredProducts.Any())
        //    throw new NoContentException("No se encontraron productos");
        var allProducts = await _repository.GetAll<Product>();
        var products = filterredProducts!.Select(p => new ProductModel.ResponseProduct(
                p.Id,
                p.Sku,
                p.InternalCode,
                p.Name,
                p.Description,
                p.CurrentUnitPrice,
                p.StockQuantity,
                p.IsActive))
            .OrderBy(p => p.Sku)
            .Skip(((request.PageNumber??1) - 1) * request.PageSize ?? 0)
            .Take(request.PageSize ?? filterredProducts!.Count());

        return new ProductModel.ResponsePagination(products.ToList(), filterredProducts!.Count(), allProducts!.Count());
    }

    public async Task<ProductModel.ResponseProduct> AddProduct(ProductModel.RequestProduct request)
    {
        _logger.LogInformation("Solicitud de agregar productos");
        Validations.GeneralValidations.ValidateNotNull(request, nameof(request));
        await ProductValidations.ValidateAddedProduct(request, _repository);

        var product = new Product(request.Sku, request.InternalCode, request.Name, request.Description, request.CurrentUnitPrice, request.StockQuantity);
        await _repository.Add(product);
        _logger.LogInformation("Creacion de producto exitosa");
        return new ProductModel.ResponseProduct(
            product.Id,
            product.Sku,
            product.InternalCode,
            product.Name,
            product.Description,
            product.CurrentUnitPrice,
            product.StockQuantity,
            product.IsActive);
    }

    public async Task<ProductModel.ResponseProduct> UpdateProduct(Guid id, ProductModel.RequestProduct request)
    {
        _logger.LogInformation("Modificacion de producto con Id: {id}", id);
        GeneralValidations.ValidateNotNull(request, nameof(request));
        await ProductValidations.ValidateExistingProduct(id, _repository);
        ProductValidations.ValidateProduct(request);
        var product = await _repository.First<Product>(p => p.Id == id);

        product!.Sku = request.Sku;
        product.InternalCode = request.InternalCode;
        product.Name = request.Name;
        product.Description = request.Description;
        product.CurrentUnitPrice = request.CurrentUnitPrice;
        product.StockQuantity = request.StockQuantity;

        await ProductValidations.ValidateUpdatedProduct(product!, _repository);

        var updated = await _repository.Update(product);
        _logger.LogInformation("Modificacion de producto exitosa");
        return new ProductModel.ResponseProduct(
            updated.Id,
            updated.Sku,
            updated.InternalCode,
            updated.Name,
            updated.Description,
            updated.CurrentUnitPrice,
            updated.StockQuantity,
            updated.IsActive
        );
    }
    public async Task<ProductModel.ResponseProduct?> DeactivateProduct(Guid id)
    {
        _logger.LogInformation("Desactivacion de producto con Id: {id}", id);
        var product = await _repository.GetById<Product>(id);
        await ProductValidations.ValidateExistingProduct(id, _repository);
        ProductValidations.ValidateActiveProduct(product!);
        product!.IsActive = false;
        var updated = await _repository.Update(product);
        _logger.LogInformation("Desactivacion de producto exitosa");
        return new ProductModel.ResponseProduct(
            updated.Id,
            updated.Sku,
            updated.InternalCode,
            updated.Name,
            updated.Description,
            updated.CurrentUnitPrice,
            updated.StockQuantity,
            updated.IsActive
        );
    }
}
