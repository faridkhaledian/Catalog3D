using System.ComponentModel.DataAnnotations;

namespace Catalog3D.Application.Products.Dtos;
public class ProductAttributeDto
{
    [Required, StringLength(100)]
    public string Key { get; set; } = string.Empty;
    [Required, StringLength(500)]
    public string Value { get; set; } = string.Empty;
}