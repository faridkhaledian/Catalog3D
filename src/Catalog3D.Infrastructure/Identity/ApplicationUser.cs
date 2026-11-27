using Microsoft.AspNetCore.Identity;

namespace Catalog3D.Infrastructure.Identity;

// فعلاً فیلد اضافه‌ای نیاز نداریم؛ ولی داشتن کلاس اختصاصی بعداً افزودن فیلد (مثل نام کامل) را بدون دردسر می‌کند
public class ApplicationUser : IdentityUser
{
}