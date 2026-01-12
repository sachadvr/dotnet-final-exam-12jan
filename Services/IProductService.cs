using dotnet.Models;

namespace dotnet.Services;

internal interface IProductService
{
    IEnumerable<Product> GetAllProducts();
    Product? GetProductById(int id);
    bool IsStockAvailable(int productId, int quantity);
    void UpdateStock(int productId, int quantity);
}
