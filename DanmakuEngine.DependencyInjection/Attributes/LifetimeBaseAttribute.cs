namespace DanmakuEngine.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class LifetimeBaseAttribute : Attribute
{
    internal Type ServiceType { get; }
    internal Type ImplType { get; }

    internal bool IsMapped => ServiceType != ImplType;

    internal bool IsDefault => ServiceType == ImplType;

    // Make the constructor internal so that the implType must derive from the serviceType with constraints
    internal LifetimeBaseAttribute(Type serviceType, Type implType)
    {
        ServiceType = serviceType;
        ImplType = implType;
    }

    internal LifetimeBaseAttribute(Type serviceType)
    {
        ImplType = ServiceType = serviceType;
    }
}

