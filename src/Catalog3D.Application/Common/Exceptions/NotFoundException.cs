namespace Catalog3D.Application.Common.Exceptions;
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}