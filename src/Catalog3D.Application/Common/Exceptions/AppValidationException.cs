using System;

namespace Catalog3D.Application.Common.Exceptions;
// خطایی که پیامش برای نمایش به کاربر امن است (مثلاً «فایل GLB معتبر نیست»)
public class AppValidationException : Exception
{
    public AppValidationException(string message) : base(message) { }
}