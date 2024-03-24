namespace DanmakuEngine.DependencyInjection;

public class ServiceAccessor
{
    public readonly CallSiteKind Kind;

    private readonly Delegate _factory;

    public ServiceAccessor(FactoryDelegate factory)
    {
        Kind = CallSiteKind.Factory;
        _factory = factory;
    }

    public ServiceAccessor(ParamlessFactoryDelegate factory)
    {
        Kind = CallSiteKind.ParamlessFactory;
        _factory = factory;
    }

    public object Create(ServiceProviderBase provider)
        => Kind switch
        {
            CallSiteKind.Factory => ((FactoryDelegate)_factory)(provider),
            CallSiteKind.ParamlessFactory => ((ParamlessFactoryDelegate)_factory)(),
            _ => throw new InvalidOperationException("Invalid CallSiteKind"),
        };
}