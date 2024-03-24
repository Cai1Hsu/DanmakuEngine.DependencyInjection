namespace DanmakuEngine.DependencyInjection;

/// <summary>
/// Mark a constructor to be used for dependency injection creation.
/// Only one constructor can be marked with this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
public sealed class DependencyInjectionCreationAttribute : Attribute
{
    public DependencyInjectionCreationAttribute()
    {
    }
}