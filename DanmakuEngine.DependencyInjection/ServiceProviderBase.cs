using System.Collections.Generic;

namespace DanmakuEngine.DependencyInjection;

public abstract class ServiceProviderBase
{
    internal readonly IDictionary<Type, ServiceAccessor> _accessors;

    private readonly ScopedEngine _rootScope;

    private readonly ScopedEngine _scoped;


    // TODO: What if a singleton requires a scoped service?
}

public abstract class ServiceProviderBase<T> : ServiceProviderBase
    where T : ServiceProviderBase
{
}
