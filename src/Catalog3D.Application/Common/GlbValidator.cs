using Catalog3D.Application.Common.Exceptions;
namespace Catalog3D.Application.Common;

public static class GlbValidator
{
    public const long MaxSizeBytes = 200L * 1024 * 1024; // 200MB

    public static void Validate(FileUpload file)
    {
        if (file is null || file.Length == 0)
            throw new AppValidationException("فایل مدل سه‌بعدی انتخاب نشده است.");

        if (file.Length > MaxSizeBytes)
            throw new AppValidationException("حجم فایل بیشتر از حد مجاز (۲۰۰ مگابایت) است.");

        if (!string.Equals(Path.GetExtension(file.FileName), ".glb", StringComparison.OrdinalIgnoreCase))
            throw new AppValidationException("فقط فایل با پسوند GLB مجاز است.");

        if (!file.Content.CanSeek)
            throw new InvalidOperationException("Stream باید قابل Seek باشد.");

        // چهار بایت اول هر فایل GLB معتبر، متن "glTF" است
        var header = new byte[4];
        file.Content.Position = 0;
        var read = file.Content.Read(header, 0, 4);
        file.Content.Position = 0;

        if (read < 4 || header[0] != 0x67 || header[1] != 0x6C || header[2] != 0x54 || header[3] != 0x46)
            throw new AppValidationException("محتوای فایل یک GLB معتبر نیست.");
    }
}