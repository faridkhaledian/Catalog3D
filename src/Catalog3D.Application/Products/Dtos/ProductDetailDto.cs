namespace Catalog3D.Application.Products.Dtos;
//For display on the website
public class ProductDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string ModelUrl { get; set; } = string.Empty;
    public List<ProductAttributeDto> Attributes { get; set; } = new();
}