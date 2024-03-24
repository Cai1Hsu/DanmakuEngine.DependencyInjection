namespace DanmakuEngine.DependencyInjection;

public interface IRegistry
{
    ServiceProviderBase Build();
}

public interface IRegistry<T>
    where T : ServiceProviderBase
{
    ServiceProviderBase<T> Build();
}
