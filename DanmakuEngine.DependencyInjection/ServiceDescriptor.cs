namespace DanmakuEngine.DependencyInjection;

internal class ServiceDescriptor
{
    internal Type ImplType { get; }

    internal ServiceDescriptor(Type implementationType)
    {
        ImplType = implementationType;
    }
}
