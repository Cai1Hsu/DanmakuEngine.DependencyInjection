namespace DanmakuEngine.DependencyInjection;

/// <summary>
/// Use <see cref="TransientAttribute{TService, TImpl}"/> or <see cref="TransientAttribute{TImpl}"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class TransientAttribute : LifetimeBaseAttribute
{
    internal TransientAttribute(Type serviceType, Type implType)
        : base(serviceType, implType)
    {
    }

    internal TransientAttribute(Type serviceType)
        : base(serviceType)
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class TransientAttribute<TService, TImpl>
    : TransientAttribute
    where TService : class
    where TImpl : class, TService
{
    public TransientAttribute()
        : base(typeof(TService), typeof(TImpl))
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class TransientAttribute<TImpl>
    : TransientAttribute
    where TImpl : class
{
    public TransientAttribute()
        : base(typeof(TImpl))
    {
    }

}
