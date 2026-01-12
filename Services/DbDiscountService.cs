using dotnet.Data;
using dotnet.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet.Services;

internal sealed class DbDiscountService(EcommerceDbContext dbContext) : IDiscountService
{
    private const double MinimumOrderForPromoCode = 50.0;
    private const double AutomaticDiscountThreshold = 100.0;
    private const double AutomaticDiscountRate = 0.05;

    public (IList<Discount> discounts, string? error) CalculateDiscounts(string? promoCode, double subtotal)
    {
        var discounts = new List<Discount>();

        if (subtotal > AutomaticDiscountThreshold)
        {
            discounts.Add(new Discount
            {
                Type = "order",
                Value = subtotal * AutomaticDiscountRate
            });
        }

        if (!string.IsNullOrWhiteSpace(promoCode))
        {
            var (promoDiscount, error) = ValidateAndApplyPromoCode(promoCode, subtotal);

            if (error is not null)
            {
                return (new List<Discount>(), error);
            }

            if (promoDiscount is not null)
            {
                discounts.Add(promoDiscount);
            }
        }

        return (discounts, null);
    }

    private (Discount? discount, string? error) ValidateAndApplyPromoCode(string promoCode, double subtotal)
    {
        if (subtotal <= MinimumOrderForPromoCode)
        {
            return (null, "Les codes promos ne sont valables qu'à partir de 50€ d'achat");
        }

        var normalizedCode = promoCode.Trim().ToUpperInvariant();
        var promo = dbContext.PromoCodes
            .AsNoTracking()
            .FirstOrDefault(p => p.Code == normalizedCode);

        if (promo is null)
        {
            return (null, "Le code promo est invalide");
        }

        var discount = new Discount
        {
            Type = "order",
            Value = subtotal * promo.DiscountRate
        };

        return (discount, null);
    }
}
