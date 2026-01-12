using dotnet.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet.Data;

internal sealed class EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
}
