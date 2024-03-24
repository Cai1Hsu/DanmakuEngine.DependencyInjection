namespace DanmakuEngine.DependencyInjection;

/// <summary>
/// Mark a user defined type as a service provider
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ServiceProviderAttribute : Attribute
{
}
