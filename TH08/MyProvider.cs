using DanmakuEngine.DependencyInjection;

[ServiceProvider]
// [Singleton<string>]
// [Transient<IService, Service<string>>]
public partial class MyProvider
{
}

interface IService
{
}

public class Service<T> : IService
    where T : Random
{
    private readonly T _module;

    public Service(T module)
    {
        _module = module;
    }

    public int Next()
        => _module.Next();
}
