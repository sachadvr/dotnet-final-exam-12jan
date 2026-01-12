namespace dotnet.Models;

public sealed class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public double Price { get; set; }
    public int Stock { get; set; }
}
