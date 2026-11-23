using Catalog3D.Application.Common;
using Catalog3D.Application.Products.Dtos;

namespace Catalog3D.Application.Products;
public interface IProductService
{
    // --- سایت عمومی ---
    Task<List<ProductListItemDto>> GetActiveProductsAsync(CancellationToken ct = default);
    Task<ProductDetailDto?> GetActiveProductBySlugAsync(string slug, CancellationToken ct = default);

    // --- پنل مدیریت ---
    Task<List<ProductListItemDto>> GetAllProductsAsync(CancellationToken ct = default);
    Task<ProductEditDto?> GetForEditAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(ProductEditDto dto, FileUpload model, CancellationToken ct = default);
    Task UpdateAsync(ProductEditDto dto, FileUpload? newModel, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}