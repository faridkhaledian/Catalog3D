using System.ComponentModel.DataAnnotations;
//For the create and edit form
namespace Catalog3D.Application.Products.Dtos;
public class ProductEditDto
{
    public int Id { get; set; }
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;
    [Required, StringLength(200)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$",
        ErrorMessage = "Slug فقط می‌تواند شامل حروف کوچک انگلیسی، عدد و خط تیره باشد.")]
    public string Slug { get; set; } = string.Empty;
    [StringLength(1000)]
    public string? Description { get; set; }
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    // فقط هنگام ویرایش پر می‌شود (برای نمایش مدل فعلی)
    public string? CurrentModelFileName { get; set; }

    public List<ProductAttributeDto> Attributes { get; set; } = new();
}