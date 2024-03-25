namespace DanmakuEngine.DependencyInjection;

/// <summary>
/// Mark a static method as a factory method.
/// Will be used to create an instance of <see cref="{T}"/>.
/// This method has higher priority than the constructor which means if a factory method is defined, the constructor will not be used.
/// The factory method must return <see cref="{T}"/> and receive only one parameter of type <see cref="ServiceProviderBase"/>
/// </summary>
/// <example>
/// <code>
/// [FactoryMethod&lt;Service&gt;]
/// public static Service Create(ServiceProviderBase provider)
///     => new Service(provider.GetRequired&lt;Dependency&gt;());
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class FactoryMethodAttribute<T> : Attribute
{
    public FactoryMethodAttribute()
    {
    }
}
