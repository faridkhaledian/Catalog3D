using Catalog3D.Application.Common;
using Catalog3D.Application.Common.Exceptions;
using Catalog3D.Application.Common.Interfaces;
using Catalog3D.Application.Products;
using Catalog3D.Application.Products.Dtos;
using Catalog3D.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog3D.Infrastructure.Services;

public class ProductService : IProductService
{
    private const string ModelExtension = ".glb";

    private readonly IApplicationDbContext _db;
    private readonly IFileStorage _fileStorage;

    public ProductService(IApplicationDbContext db, IFileStorage fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    // ===================== سایت عمومی =====================
    public async Task<List<ProductListItemDto>> GetActiveProductsAsync(CancellationToken ct = default)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Price = p.Price,
                IsActive = p.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<ProductDetailDto?> GetActiveProductBySlugAsync(string slug, CancellationToken ct = default)
    {
        var normalizedSlug = NormalizeSlug(slug);

        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.IsActive && p.Slug == normalizedSlug, ct);

        if (product is null)
            return null;

        return new ProductDetailDto
        {
            Id = product.Id,
            Title = product.Title,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            ModelUrl = _fileStorage.GetUrl(product.ModelFileName),
            Attributes = MapAttributesToDtos(product.Attributes)
        };
    }

    // ===================== پنل مدیریت =====================

    public async Task<List<ProductListItemDto>> GetAllProductsAsync(CancellationToken ct = default)
    {
        return await _db.Products
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Price = p.Price,
                IsActive = p.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<ProductEditDto?> GetForEditAsync(int id, CancellationToken ct = default)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
            return null;

        return new ProductEditDto
        {
            Id = product.Id,
            Title = product.Title,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            IsActive = product.IsActive,
            CurrentModelFileName = product.ModelFileName,
            Attributes = MapAttributesToDtos(product.Attributes)
        };
    }

    public async Task<int> CreateAsync(ProductEditDto dto, FileUpload model, CancellationToken ct = default)
    {
        GlbValidator.Validate(model);

        var slug = NormalizeSlug(dto.Slug);
        await EnsureSlugIsUniqueAsync(slug, excludeProductId: 0, ct);

        var storedFileName = await _fileStorage.SaveAsync(model.Content, ModelExtension, ct);

        try
        {
            var product = new Product
            {
                Title = dto.Title.Trim(),
                Slug = slug,
                Description = NormalizeText(dto.Description),
                Price = dto.Price,
                IsActive = dto.IsActive,
                ModelFileName = storedFileName,
                CreatedAt = DateTime.UtcNow,
                Attributes = MapAttributesToEntities(dto.Attributes)
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync(ct);

            return product.Id;
        }
        catch
        {
            await _fileStorage.DeleteAsync(storedFileName, CancellationToken.None);
            throw;
        }
    }

    public async Task UpdateAsync(ProductEditDto dto, FileUpload? newModel, CancellationToken ct = default)
    {
        var product = await _db.Products
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Id == dto.Id, ct)
            ?? throw new NotFoundException("محصول مورد نظر پیدا نشد.");

        if (newModel is not null)
            GlbValidator.Validate(newModel);

        var slug = NormalizeSlug(dto.Slug);
        await EnsureSlugIsUniqueAsync(slug, product.Id, ct);

        var oldFileName = product.ModelFileName;
        string? newFileName = null;

        if (newModel is not null)
            newFileName = await _fileStorage.SaveAsync(newModel.Content, ModelExtension, ct);

        try
        {
            product.Title = dto.Title.Trim();
            product.Slug = slug;
            product.Description = NormalizeText(dto.Description);
            product.Price = dto.Price;
            product.IsActive = dto.IsActive;

            if (newFileName is not null)
                product.ModelFileName = newFileName;

            _db.ProductAttributes.RemoveRange(product.Attributes.ToList());

            foreach (var attribute in MapAttributesToEntities(dto.Attributes))
            {
                attribute.ProductId = product.Id;
                _db.ProductAttributes.Add(attribute);
            }

            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            if (newFileName is not null)
                await _fileStorage.DeleteAsync(newFileName, CancellationToken.None);

            throw;
        }

        // فقط بعد از موفقیت دیتابیس، فایل قدیمی پاک می‌شود
        if (newFileName is not null)
            await _fileStorage.DeleteAsync(oldFileName, CancellationToken.None);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("محصول مورد نظر پیدا نشد.");

        var fileName = product.ModelFileName;

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);

        await _fileStorage.DeleteAsync(fileName, CancellationToken.None);
    }

    // ===================== متدهای کمکی =====================

    private async Task EnsureSlugIsUniqueAsync(string slug, int excludeProductId, CancellationToken ct)
    {
        var exists = await _db.Products
            .AnyAsync(p => p.Slug == slug && p.Id != excludeProductId, ct);

        if (exists)
            throw new AppValidationException("این Slug قبلاً برای محصول دیگری استفاده شده است.");
    }

    private static string NormalizeSlug(string? slug)
    {
        return (slug ?? string.Empty).Trim().ToLowerInvariant();
    }

    private static string? NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
    
    private static List<ProductAttribute> MapAttributesToEntities(IEnumerable<ProductAttributeDto>? attributes)
    {
        if (attributes is null)
            return new List<ProductAttribute>();

        return attributes
            .Where(a => !string.IsNullOrWhiteSpace(a.Key) && !string.IsNullOrWhiteSpace(a.Value))
            .Select(a => new ProductAttribute
            {
                Key = a.Key.Trim(),
                Value = a.Value.Trim()
            })
            .ToList();
    }

    private static List<ProductAttributeDto> MapAttributesToDtos(IEnumerable<ProductAttribute> attributes)
    {
        return attributes
            .OrderBy(a => a.Id) 
            .Select(a => new ProductAttributeDto { Key = a.Key, Value = a.Value })
            .ToList();
    }
}