using dotnet.Models;

namespace dotnet.Services;

internal interface IDiscountService
{
    (IList<Discount> discounts, string? error) CalculateDiscounts(string? promoCode, double subtotal);
}
