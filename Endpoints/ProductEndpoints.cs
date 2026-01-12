using dotnet.Services;

namespace dotnet.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        // GET /products - Retourne la liste de tous les produits
        app.MapGet("/products", (IProductService productService) =>
        {
            var products = productService.GetAllProducts();
            return Results.Ok(products);
        })
        .WithName("GetProducts");
    }
}
