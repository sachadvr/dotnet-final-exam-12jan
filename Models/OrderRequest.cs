namespace dotnet.Models;

public sealed class OrderRequest
{
    public required IList<OrderProductItem> Products { get; set; }
    public string? PromoCode { get; set; }
}

public sealed class OrderProductItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
}
