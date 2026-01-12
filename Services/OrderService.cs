using dotnet.Models;

namespace dotnet.Services;

internal sealed class OrderService(
    IProductService productService,
    IDiscountService discountService) : IOrderService
{
    public Task<(OrderResponse? response, IList<string> errors)> CreateOrderAsync(OrderRequest orderRequest)
    {
        var errors = new List<string>();
        var orderProducts = new List<OrderProductDetail>();
        var subtotal = CalculateSubtotal(orderRequest, orderProducts, errors);

        if (errors.Count > 0)
        {
            return Task.FromResult<(OrderResponse? response, IList<string> errors)>((null, errors));
        }

        var (discounts, discountError) = discountService.CalculateDiscounts(orderRequest.PromoCode, subtotal);
        
        if (discountError is not null)
        {
            errors.Add(discountError);
            return Task.FromResult<(OrderResponse? response, IList<string> errors)>((null, errors));
        }

        var total = CalculateFinalTotal(subtotal, discounts);

        UpdateProductStock(orderRequest);

        var response = new OrderResponse
        {
            Products = orderProducts,
            Discounts = discounts,
            Total = total
        };

        return Task.FromResult<(OrderResponse? response, IList<string> errors)>((response, new List<string>()));
    }

    private double CalculateSubtotal(
        OrderRequest orderRequest,
        IList<OrderProductDetail> orderProducts,
        IList<string> errors)
    {
        double subtotal = 0;

        foreach (var item in orderRequest.Products)
        {
            var product = productService.GetProductById(item.Id);

            if (!ValidateProduct(product, item, errors))
            {
                continue;
            }

            var unitPrice = ApplyQuantityDiscount(product!.Price, item.Quantity);

            orderProducts.Add(new OrderProductDetail
            {
                Id = product.Id,
                Quantity = item.Quantity,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock
            });

            subtotal += unitPrice * item.Quantity;
        }

        return subtotal;
    }

    private bool ValidateProduct(Product? product, OrderProductItem item, IList<string> errors)
    {
        if (product is null)
        {
            errors.Add($"Le produit avec l'identifiant {item.Id} n'existe pas");
            return false;
        }

        if (!ValidateStock(product, item.Quantity, errors))
        {
            return false;
        }

        return true;
    }

    private static bool ValidateStock(Product product, int requestedQuantity, IList<string> errors)
    {
        // Règle métier : Vérifier si la quantité demandée est disponible en stock
        if (requestedQuantity > product.Stock)
        {
            var pluriel = product.Stock > 1 ? "exemplaires" : "exemplaire";
            errors.Add($"Il ne reste que {product.Stock} {pluriel} pour le produit {product.Name}");
            return false;
        }

        return true;
    }

    private static double ApplyQuantityDiscount(double price, int quantity)
    {
        // Règle métier : Remise de 10% si quantité > 5
        return quantity > 5 ? price * 0.90 : price;
    }

    private static double CalculateFinalTotal(double subtotal, IEnumerable<Discount> discounts)
    {
        var totalDiscount = discounts.Sum(d => d.Value);
        return subtotal - totalDiscount;
    }

    private void UpdateProductStock(OrderRequest orderRequest)
    {
        // Règle métier : Mettre à jour les quantités en stock pour refléter les produits réservés
        foreach (var item in orderRequest.Products)
        {
            productService.UpdateStock(item.Id, item.Quantity);
        }
    }
}
