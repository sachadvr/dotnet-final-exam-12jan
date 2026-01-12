using dotnet.Models;
using dotnet.Services;
using Microsoft.AspNetCore.Mvc;

namespace dotnet.Endpoints;

public static class OrderEndpoints
{
    /// <summary>
    /// /orders - Creation d'une nouvelle commande
    /// </summary>
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/orders", async (
            [FromBody] OrderRequest orderRequest,
            [FromServices] IOrderService orderService) =>
        {
            var (response, errors) = await orderService.CreateOrderAsync(orderRequest);

            if (errors.Count > 0)
            {
                return Results.BadRequest(new ErrorResponse { Errors = errors });
            }

            return Results.Ok(response);
        })
        .WithName("CreateOrder");
    }
}
