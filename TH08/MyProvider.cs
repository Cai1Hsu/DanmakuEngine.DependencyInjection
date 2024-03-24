using DanmakuEngine.DependencyInjection;

[ServiceProvider]
// [Transient<string>]
[Singleton<IMyProvider, MyProvider<string>>]
public partial class MyProvider
{
}

public interface IMyProvider
{
}

public interface MyProvider<T> : IMyProvider
{
    // [DependencyInjectionCreation]
    // public MyProvider(T a)
    // {
    // }

    // [DependencyInjectionCreation]
    // public MyProvider(string a)
    // {

    // }

    // public MyProvider()
    // {
    // }
}
