using dotnet.Models;

namespace dotnet.Services;

[Obsolete("Utilise DbProductService à la place de ProductService.")]
internal sealed class ProductService : IProductService
{
    private readonly List<Product> _products;

    public ProductService()
    {
        _products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Ordinateur Portable",
                Price = 899.99,
                Stock = 15
            },
            new Product
            {
                Id = 2,
                Name = "Souris Sans Fil",
                Price = 29.99,
                Stock = 50
            },
            new Product
            {
                Id = 3,
                Name = "Clavier Mécanique",
                Price = 79.99,
                Stock = 30
            },
            new Product
            {
                Id = 4,
                Name = "Écran 27 pouces",
                Price = 299.99,
                Stock = 20
            },
            new Product
            {
                Id = 5,
                Name = "Webcam HD",
                Price = 59.99,
                Stock = 40
            }
        };
    }

    public IEnumerable<Product> GetAllProducts()
    {
        return _products;
    }

    public Product? GetProductById(int id)
    {
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public bool IsStockAvailable(int productId, int quantity)
    {
        var product = GetProductById(productId);
        return product is not null && product.Stock >= quantity;
    }

    public void UpdateStock(int productId, int quantity)
    {
        var product = GetProductById(productId);
        if (product is not null)
        {
            product.Stock -= quantity;
        }
    }
}
