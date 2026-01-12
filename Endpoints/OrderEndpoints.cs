using dotnet.Models;
using dotnet.Services;
using Microsoft.AspNetCore.Mvc;

namespace dotnet.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        // POST /orders - Crée une nouvelle commande
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
