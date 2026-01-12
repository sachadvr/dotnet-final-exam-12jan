using dotnet.Services;

namespace dotnet.Endpoints;

public static class ProductEndpoints
{
    /// <summary>
    /// /products - Retourne la liste de tous les produits
    /// </summary>
    public static void MapProductEndpoints(this WebApplication app)
    {
        app.MapGet("/products", (IProductService productService) =>
        {
            var products = productService.GetAllProducts();
            return Results.Ok(products);
        })
        .WithName("GetProducts");
    }
}
