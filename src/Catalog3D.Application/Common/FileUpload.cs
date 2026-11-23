namespace Catalog3D.Application.Common;
public class FileUpload
{
    public Stream Content { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public long Length { get; init; }
}