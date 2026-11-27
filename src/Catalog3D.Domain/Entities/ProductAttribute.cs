using System;

namespace Catalog3D.Domain.Entities;
public class ProductAttribute
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Product Product { get; set; } = null!;
}