using Catalog3D.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace Catalog3D.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<ProductAttribute> ProductAttributes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}