using dotnet.Models;

namespace dotnet.Services;

internal sealed class DiscountService : IDiscountService
{
    private const double MinimumOrderForPromoCode = 50.0;
    private const double AutomaticDiscountThreshold = 100.0;
    private const double AutomaticDiscountRate = 0.05;

    public (IList<Discount> discounts, string? error) CalculateDiscounts(string? promoCode, double subtotal)
    {
        var discounts = new List<Discount>();

        // Règle métier : Remise automatique de 5% si total > 100€
        if (subtotal > AutomaticDiscountThreshold)
        {
            discounts.Add(new Discount
            {
                Type = "order",
                Value = subtotal * AutomaticDiscountRate
            });
        }

        // Validation et application des codes promo
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

    private static (Discount? discount, string? error) ValidateAndApplyPromoCode(string promoCode, double subtotal)
    {
        // Règle 2 : Les codes promos sont valides seulement si la commande dépasse 50€
        if (subtotal <= MinimumOrderForPromoCode)
        {
            return (null, "Les codes promos ne sont valables qu'à partir de 50€ d'achat");
        }

        // Règle 1 : Valider le code promo
        var discountRate = GetPromoCodeDiscountRate(promoCode);

        if (discountRate == 0)
        {
            return (null, "Le code promo est invalide");
        }

        // Règle 3 : Application additive avec les autres remises
        var discount = new Discount
        {
            Type = "order",
            Value = subtotal * discountRate
        };

        return (discount, null);
    }

    private static double GetPromoCodeDiscountRate(string promoCode)
    {
        return promoCode.ToUpperInvariant() switch
        {
            "DISCOUNT 20" => 0.20,
            "DISCOUNT 10" => 0.10,
            _ => 0
        };
    }
}
