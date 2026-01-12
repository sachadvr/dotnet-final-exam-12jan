using dotnet.Data;
using dotnet.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet.Services;

internal sealed class DbProductService(EcommerceDbContext dbContext) : IProductService
{
    public IEnumerable<Product> GetAllProducts()
    {
        return dbContext.Products.AsNoTracking().ToList();
    }

    public Product? GetProductById(int id)
    {
        return dbContext.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
    }

    public bool IsStockAvailable(int productId, int quantity)
    {
        var product = GetProductById(productId);
        return product is not null && product.Stock >= quantity;
    }

    public void UpdateStock(int productId, int quantity)
    {
        var product = dbContext.Products.FirstOrDefault(p => p.Id == productId);
        if (product is null)
        {
            return;
        }

        product.Stock -= quantity;
        dbContext.SaveChanges();
    }
}
