using System.Text.RegularExpressions;
using Catalog3D.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Catalog3D.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private const string UrlPrefix = "/uploads/models/";
    // فقط پسوندهای ساده مثل .glb مجاز است؛ جلوگیری از مسیرهایی مثل "../x"
    private static readonly Regex SafeExtension = new(@"^\.[a-z0-9]{1,10}$", RegexOptions.Compiled);

    private readonly string _folder;

    public LocalFileStorage(IWebHostEnvironment environment)
    {
        var webRoot = string.IsNullOrEmpty(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;

        _folder = Path.Combine(webRoot, "uploads", "models");
        Directory.CreateDirectory(_folder);
    }

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var safeExtension = NormalizeExtension(extension);

        var storedFileName = $"{Guid.NewGuid():N}{safeExtension}";
        var fullPath = Path.Combine(_folder, storedFileName);

        try
        {
            await using var fileStream = new FileStream(
                fullPath,
                FileMode.CreateNew,     
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await content.CopyToAsync(fileStream, cancellationToken);
        }
        catch
        {
            TryDelete(fullPath);
            throw;
        }

        return storedFileName;
    }

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
            return Task.CompletedTask;

        // محافظت در برابر Path Traversal: فقط یک «نام فایل» ساده پذیرفته می‌شود، نه مسیر
        if (!string.Equals(Path.GetFileName(storedFileName), storedFileName, StringComparison.Ordinal))
            throw new ArgumentException("نام فایل نامعتبر است.", nameof(storedFileName));

        TryDelete(Path.Combine(_folder, storedFileName));
        return Task.CompletedTask;
    }

    public string GetUrl(string storedFileName)
    {
        return UrlPrefix + Uri.EscapeDataString(storedFileName);
    }

    private static string NormalizeExtension(string extension)
    {
        var value = (extension ?? string.Empty).Trim().ToLowerInvariant();

        if (!value.StartsWith('.'))
            value = "." + value;

        if (!SafeExtension.IsMatch(value))
            throw new ArgumentException("پسوند فایل نامعتبر است.", nameof(extension));

        return value;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
            // فایل در حال استفاده است؛ نبودن حذف، نباید عملیات اصلی را خراب کند
        }
        catch (UnauthorizedAccessException)
        {
            // دسترسی حذف نداریم؛ همین‌طور نادیده گرفته می‌شود
        }
    }
}