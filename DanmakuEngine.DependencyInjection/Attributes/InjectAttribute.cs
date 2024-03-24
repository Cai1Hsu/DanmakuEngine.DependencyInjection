namespace DanmakuEngine.DependencyInjection;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class InjectAttribute : Attribute
{
}
