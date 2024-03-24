namespace DanmakuEngine.DependencyInjection;

public interface IScope
{
    ServiceProviderBase ServiceProvider { get; }

    IScope CreateScope();
}

public interface IScope<T> : IScope
    where T : ServiceProviderBase
{
}
