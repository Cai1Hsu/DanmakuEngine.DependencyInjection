namespace DanmakuEngine.DependencyInjection;

/// <summary>
/// Use <see cref="SingletonAttribute{TService, TImpl}"/> or <see cref="SingletonAttribute{TService}"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class SingletonAttribute : LifetimeBaseAttribute
{
    internal SingletonAttribute(Type serviceType, Type implType)
        : base(serviceType, implType)
    {
    }

    internal SingletonAttribute(Type serviceType)
        : base(serviceType)
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class SingletonAttribute<TService, TImpl>
    : SingletonAttribute
    where TService : class
    where TImpl : class, TService
{
    public SingletonAttribute()
        : base(typeof(TService), typeof(TImpl))
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class SingletonAttribute<TService>
    : SingletonAttribute
    where TService : class
{
    public SingletonAttribute()
        : base(typeof(TService))
    {
    }
}
