using dotnet.Models;

namespace dotnet.Data;

internal static class EcommerceDbSeeder
{
    public static void Seed(EcommerceDbContext dbContext)
    {
        dbContext.Database.EnsureCreated();

        if (!dbContext.Products.Any())
        {
            dbContext.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Ordinateur Portable",
                    Price = 899.99,
                    Stock = 15
                },
                new Product
                {
                    Id = 2,
                    Name = "Souris Sans Fil",
                    Price = 29.99,
                    Stock = 50
                },
                new Product
                {
                    Id = 3,
                    Name = "Clavier Mécanique",
                    Price = 79.99,
                    Stock = 30
                },
                new Product
                {
                    Id = 4,
                    Name = "Écran 27 pouces",
                    Price = 299.99,
                    Stock = 20
                },
                new Product
                {
                    Id = 5,
                    Name = "Webcam HD",
                    Price = 59.99,
                    Stock = 40
                }
            );
        }

        if (!dbContext.PromoCodes.Any())
        {
            dbContext.PromoCodes.AddRange(
                new PromoCode
                {
                    Id = 1,
                    Code = "DISCOUNT 10",
                    DiscountRate = 0.10
                },
                new PromoCode
                {
                    Id = 2,
                    Code = "DISCOUNT 20",
                    DiscountRate = 0.20
                }
            );
        }

        dbContext.SaveChanges();
    }
}
