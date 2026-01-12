using dotnet.Models;
using dotnet.Services;
using Xunit;

namespace dotnet.Tests.Services;

public sealed class OrderServiceTests
{
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IDiscountService _discountService;

    public OrderServiceTests()
    {
        _productService = new ProductService();
        _discountService = new DiscountService();
        _orderService = new OrderService(_productService, _discountService);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidProducts_ShouldSucceed()
    {
        var orderRequest = new OrderRequest
        {
            Products = new List<OrderProductItem>
            {
                new() { Id = 1, Quantity = 2 },
                new() { Id = 2, Quantity = 1 }
            },
            PromoCode = null
        };

        var (response, errors) = await _orderService.CreateOrderAsync(orderRequest);

        Assert.Empty(errors);
        Assert.NotNull(response);
        Assert.Equal(2, response.Products.Count);
        Assert.True(response.Total > 0);
    }

    [Fact]
    public async Task CreateOrderAsync_WithNonExistentProduct_ShouldReturnError()
    {
        var orderRequest = new OrderRequest
        {
            Products = new List<OrderProductItem>
            {
                new() { Id = 999, Quantity = 1 }
            },
            PromoCode = null
        };

        var (response, errors) = await _orderService.CreateOrderAsync(orderRequest);

        Assert.Null(response);
        Assert.Single(errors);
        Assert.Contains("Le produit avec l'identifiant 999 n'existe pas", errors);
    }

    [Fact]
    public async Task CreateOrderAsync_WithInsufficientStock_ShouldReturnError()
    {
        var orderRequest = new OrderRequest
        {
            Products = new List<OrderProductItem>
            {
                new() { Id = 1, Quantity = 1000 } // Stock insuffisant
            },
            PromoCode = null
        };

        var (response, errors) = await _orderService.CreateOrderAsync(orderRequest);

        Assert.Null(response);
        Assert.Single(errors);
        Assert.Contains("Il ne reste que", errors[0]);
        Assert.Contains("exemplaire", errors[0]);
    }

    [Fact]
    public async Task CreateOrderAsync_WithMultipleErrors_ShouldReturnAllErrors()
    {
        var orderRequest = new OrderRequest
        {
            Products = new List<OrderProductItem>
            {
                new() { Id = 999, Quantity = 1 },  // Produit inexistant
                new() { Id = 1, Quantity = 1000 },  // Stock insuffisant
                new() { Id = 888, Quantity = 2 }    // Produit inexistant
            },
            PromoCode = null
        };

        var (response, errors) = await _orderService.CreateOrderAsync(orderRequest);

        Assert.Null(response);
        Assert.Equal(3, errors.Count);
        Assert.Contains(errors, e => e.Contains("Le produit avec l'identifiant 999 n'existe pas"));
        Assert.Contains(errors, e => e.Contains("Il ne reste que"));
        Assert.Contains(errors, e => e.Contains("Le produit avec l'identifiant 888 n'existe pas"));
    }

    [Fact]
    public async Task CreateOrderAsync_WithQuantityOver5_ShouldApply10PercentDiscount()
    {
        var orderRequest = new OrderRequest
        {
            Products = new List<OrderProductItem>
            {
                new() { Id = 2, Quantity = 6 } // Quantité > 5
            },
            PromoCode = null
        };

        var (response, errors) = await _orderService.CreateOrderAsync(orderRequest);

        Assert.Empty(errors);
        Assert.NotNull(response);
        
        // Subtotal avec remise de quantité: 29.99 * 0.90 * 6 = 161.946
        // Remise automatique de 5% car > 100€: 161.946 * 0.05 = 8.0973
        // Total final: 161.946 - 8.0973 = 153.8487
        var expectedSubtotal = 29.99 * 0.90 * 6;
        var expectedDiscount = expectedSubtotal * 0.05;
        var expectedTotal = expectedSubtotal - expectedDiscount;
        
        Assert.Single(response.Discounts);
        Assert.Equal("order", response.Discounts[0].Type);
        Assert.Equal(expectedDiscount, response.Discounts[0].Value, 2);
        Assert.Equal(expectedTotal, response.Total, 2);
    }

    [Fact]
    public async Task CreateOrderAsync_WithTotalOver100_ShouldApplyAutomatic5PercentDiscount()
    {
        var orderRequest = new OrderRequest
        {
            Products = new List<OrderProductItem>
            {
                new() { Id = 1, Quantity = 1 } // Ordinateur: 899.99€
            },
            PromoCode = null
        };

        var (response, errors) = await _orderService.CreateOrderAsync(orderRequest);

        Assert.Empty(errors);
        Assert.NotNull(response);
        Assert.Single(response.Discounts);
        Assert.Equal("order", response.Discounts[0].Type);
        Assert.Equal(899.99 * 0.05, response.Discounts[0].Value, 2);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidPromoCode_ShouldApplyPromoDiscount()
    {
        var orderRequest = new OrderRequest
        {
            Products = new List<OrderProductItem>
            {
                new() { Id = 1, Quantity = 1 } // Ordinateur: 899.99€
            },
            PromoCode = "DISCOUNT 10"
        };

        var (response, errors) = await _orderService.CreateOrderAsync(orderRequest);

        Assert.Empty(errors);
        Assert.NotNull(response);
        Assert.Equal(2, response.Discounts.Count); // Remise auto 5% + promo 10%
        
        var totalDiscount = response.Discounts.Sum(d => d.Value);
        var expectedTotal = 899.99 - totalDiscount;
        Assert.Equal(expectedTotal, response.Total, 2);
    }

    [Fact]
    public async Task CreateOrderAsync_WithInvalidPromoCode_ShouldReturnError()
    {
        var orderRequest = new OrderRequest
        {
            Products = new List<OrderProductItem>
            {
                new() { Id = 1, Quantity = 1 }
            },
            PromoCode = "INVALID_CODE"
        };

        var (response, errors) = await _orderService.CreateOrderAsync(orderRequest);

        Assert.Null(response);
        Assert.Single(errors);
        Assert.Equal("Le code promo est invalide", errors[0]);
    }

    [Fact]
    public async Task CreateOrderAsync_WithPromoCodeAndTotalUnder50_ShouldReturnError()
    {
        var orderRequest = new OrderRequest
        {
            Products = new List<OrderProductItem>
            {
                new() { Id = 2, Quantity = 1 } // Total: 29.99€ < 50€
            },
            PromoCode = "DISCOUNT 10"
        };

        var (response, errors) = await _orderService.CreateOrderAsync(orderRequest);

        Assert.Null(response);
        Assert.Single(errors);
        Assert.Equal("Les codes promos ne sont valables qu'à partir de 50€ d'achat", errors[0]);
    }

    [Fact]
    public async Task CreateOrderAsync_WithPromoCodeDISCOUNT20AndTotalOver100_ShouldCumulateDiscounts()
    {
        var orderRequest = new OrderRequest
        {
            Products = new List<OrderProductItem>
            {
                new() { Id = 4, Quantity = 1 } // Écran: 299.99€
            },
            PromoCode = "DISCOUNT 20"
        };

        var (response, errors) = await _orderService.CreateOrderAsync(orderRequest);

        Assert.Empty(errors);
        Assert.NotNull(response);
        Assert.Equal(2, response.Discounts.Count); // 5% auto + 20% promo
        
        // Remise auto: 299.99 * 0.05 = 14.9995
        // Remise promo: 299.99 * 0.20 = 59.998
        // Total remises: 74.9975
        // Total final: 299.99 - 74.9975 = 224.9925
        var totalDiscount = response.Discounts.Sum(d => d.Value);
        Assert.Equal(299.99 * 0.25, totalDiscount, 2);
        Assert.Equal(299.99 - totalDiscount, response.Total, 2);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldUpdateStock()
    {
        var productId = 3;
        var initialStock = _productService.GetProductById(productId)!.Stock;
        var quantityToOrder = 3;

        var orderRequest = new OrderRequest
        {
            Products = new List<OrderProductItem>
            {
                new() { Id = productId, Quantity = quantityToOrder }
            },
            PromoCode = null
        };

        var (response, errors) = await _orderService.CreateOrderAsync(orderRequest);

        Assert.Empty(errors);
        Assert.NotNull(response);
        
        var updatedStock = _productService.GetProductById(productId)!.Stock;
        Assert.Equal(initialStock - quantityToOrder, updatedStock);
    }
}
