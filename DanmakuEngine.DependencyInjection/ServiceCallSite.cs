namespace DanmakuEngine.DependencyInjection;

public class ServiceCallSite
{
    public CallSiteKind Kind { get; }
    public Type ServiceType { get; }
    public Type ImplementationType { get; }
}
