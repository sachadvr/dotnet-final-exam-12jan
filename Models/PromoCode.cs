namespace dotnet.Models;

public sealed class PromoCode
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public double DiscountRate { get; set; }
}
