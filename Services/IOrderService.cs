using dotnet.Models;

namespace dotnet.Services;

internal interface IOrderService
{
    Task<(OrderResponse? response, IList<string> errors)> CreateOrderAsync(OrderRequest orderRequest);
}
