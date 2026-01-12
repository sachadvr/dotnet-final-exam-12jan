using dotnet.Services;
using Xunit;

namespace dotnet.Tests.Services;

public sealed class DiscountServiceTests
{
    private readonly IDiscountService _discountService;

    public DiscountServiceTests()
    {
        _discountService = new DiscountService();
    }

    [Fact]
    public void CalculateDiscounts_WithSubtotalOver100_ShouldApplyAutomatic5PercentDiscount()
    {
        var subtotal = 150.0;

        var (discounts, error) = _discountService.CalculateDiscounts(null, subtotal);

        Assert.Null(error);
        Assert.Single(discounts);
        Assert.Equal("order", discounts[0].Type);
        Assert.Equal(7.5, discounts[0].Value); // 150 * 0.05
    }

    [Fact]
    public void CalculateDiscounts_WithSubtotalUnder100_ShouldNotApplyAutomaticDiscount()
    {
        var subtotal = 80.0;

        var (discounts, error) = _discountService.CalculateDiscounts(null, subtotal);

        Assert.Null(error);
        Assert.Empty(discounts);
    }

    [Theory]
    [InlineData("DISCOUNT 10", 200.0, 20.0)]
    [InlineData("DISCOUNT 20", 200.0, 40.0)]
    [InlineData("discount 10", 200.0, 20.0)] // Test case insensitive
    [InlineData("DISCOUNT 10", 100.0, 10.0)]
    public void CalculateDiscounts_WithValidPromoCode_ShouldApplyPromoDiscount(
        string promoCode, double subtotal, double expectedDiscount)
    {
        var (discounts, error) = _discountService.CalculateDiscounts(promoCode, subtotal);

        Assert.Null(error);
        Assert.Contains(discounts, d => d.Value == expectedDiscount);
    }

    [Fact]
    public void CalculateDiscounts_WithInvalidPromoCode_ShouldReturnError()
    {
        var subtotal = 100.0;
        var invalidPromoCode = "INVALID_CODE";

        var (discounts, error) = _discountService.CalculateDiscounts(invalidPromoCode, subtotal);

        Assert.NotNull(error);
        Assert.Equal("Le code promo est invalide", error);
        Assert.Empty(discounts);
    }

    [Theory]
    [InlineData(40.0)]
    [InlineData(50.0)]
    [InlineData(30.0)]
    public void CalculateDiscounts_WithPromoCodeAndSubtotalUnder50_ShouldReturnError(double subtotal)
    {
        var promoCode = "DISCOUNT 10";

        var (discounts, error) = _discountService.CalculateDiscounts(promoCode, subtotal);

        Assert.NotNull(error);
        Assert.Equal("Les codes promos ne sont valables qu'à partir de 50€ d'achat", error);
        Assert.Empty(discounts);
    }

    [Fact]
    public void CalculateDiscounts_WithPromoCodeAndSubtotalOver100_ShouldCumulateDiscounts()
    {
        var subtotal = 200.0;
        var promoCode = "DISCOUNT 10";

        var (discounts, error) = _discountService.CalculateDiscounts(promoCode, subtotal);

        Assert.Null(error);
        Assert.Equal(2, discounts.Count); // Remise automatique 5% + code promo 10%
        
        var totalDiscount = discounts.Sum(d => d.Value);
        Assert.Equal(30.0, totalDiscount); // 10 (5%) + 20 (10%) = 30
        
        var finalTotal = subtotal - totalDiscount;
        Assert.Equal(170.0, finalTotal); // 200 - 30 = 170
    }

    [Theory]
    [InlineData("DISCOUNT 10", 200.0, 170.0)] // 200 - (10 + 20) = 170
    [InlineData("DISCOUNT 20", 200.0, 150.0)] // 200 - (10 + 40) = 150
    [InlineData("DISCOUNT 10", 80.0, 72.0)]   // 80 - 8 = 72 (pas de remise auto)
    public void CalculateDiscounts_WithVariousScenarios_ShouldCalculateCorrectTotal(
        string promoCode, double subtotal, double expectedTotal)
    {
        var (discounts, error) = _discountService.CalculateDiscounts(promoCode, subtotal);

        Assert.Null(error);
        var totalDiscount = discounts.Sum(d => d.Value);
        var finalTotal = subtotal - totalDiscount;
        Assert.Equal(expectedTotal, finalTotal);
    }
}
