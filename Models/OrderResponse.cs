namespace dotnet.Models;

public sealed class OrderResponse
{
    public required IList<OrderProductDetail> Products { get; set; }
    public required IList<Discount> Discounts { get; set; }
    public double Total { get; set; }
}

public sealed class OrderProductDetail
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public required string Name { get; set; }
    public double Price { get; set; }
    public int Stock { get; set; }
}

public sealed class Discount
{
    public required string Type { get; set; }
    public double Value { get; set; }
}
