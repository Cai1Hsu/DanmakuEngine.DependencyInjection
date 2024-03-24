namespace DanmakuEngine.DependencyInjection;

/// <summary>
/// Use <see cref="ScopedAttribute{TService, TImpl}"/> or <see cref="ScopedAttribute{TImpl}"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class ScopedAttribute : LifetimeBaseAttribute
{
    internal ScopedAttribute(Type serviceType, Type implType)
        : base(serviceType, implType)
    {
    }

    internal ScopedAttribute(Type serviceType)
        : base(serviceType)
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ScopedAttribute<TService, TImpl>
    : ScopedAttribute
    where TService : class
    where TImpl : class, TService
{
    public ScopedAttribute()
        : base(typeof(TService), typeof(TImpl))
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ScopedAttribute<TImpl>
    : ScopedAttribute
    where TImpl : class
{
    public ScopedAttribute()
        : base(typeof(TImpl))
    {
    }
}
