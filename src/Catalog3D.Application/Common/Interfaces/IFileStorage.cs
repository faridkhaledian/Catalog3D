namespace Catalog3D.Application.Common.Interfaces;
public interface IFileStorage
{
    // فایل را ذخیره می‌کند و نام ذخیره‌شده  را برمی‌گرداند
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default);
    // فایل ذخیره‌شده را حذف می‌کند 
    Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default);
    string GetUrl(string storedFileName);
}