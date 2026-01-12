using dotnet.Data;
using dotnet.Models;
using dotnet.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace dotnet.Tests.Services;

public sealed class ProductServiceTests
{
    private readonly IProductService _productService;

    public ProductServiceTests()
    {
        var dbContext = CreateDbContext();
        _productService = new DbProductService(dbContext);
    }

    private static EcommerceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EcommerceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new EcommerceDbContext(options);
        EcommerceDbSeeder.Seed(dbContext);

        return dbContext;
    }

    [Fact]
    public void GetAllProducts_ShouldReturn5Products()
    {
        var products = _productService.GetAllProducts().ToList();
        Assert.Equal(5, products.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void GetProductById_WithValidId_ShouldReturnProduct(int id)
    {
        var product = _productService.GetProductById(id);
        Assert.NotNull(product);
        Assert.Equal(id, product.Id);
    }

    [Theory]
    [InlineData(999)]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetProductById_WithInvalidId_ShouldReturnNull(int id)
    {
        var product = _productService.GetProductById(id);
        Assert.Null(product);
    }

    [Fact]
    public void IsStockAvailable_WithSufficientStock_ShouldReturnTrue()
    {
        var productId = 1; // Ordinateur Portable avec stock de 15
        var isAvailable = _productService.IsStockAvailable(productId, 10);
        Assert.True(isAvailable);
    }

    [Fact]
    public void IsStockAvailable_WithInsufficientStock_ShouldReturnFalse()
    {
        var productId = 1; // Ordinateur Portable avec stock de 15
        var isAvailable = _productService.IsStockAvailable(productId, 20);
        Assert.False(isAvailable);
    }

    [Fact]
    public void UpdateStock_ShouldDecreaseStockQuantity()
    {
        var productId = 1;
        var initialStock = _productService.GetProductById(productId)!.Stock;
        var quantityToRemove = 5;

        _productService.UpdateStock(productId, quantityToRemove);

        var updatedStock = _productService.GetProductById(productId)!.Stock;
        Assert.Equal(initialStock - quantityToRemove, updatedStock);
    }
}
