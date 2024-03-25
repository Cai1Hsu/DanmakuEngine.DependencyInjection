namespace DanmakuEngine.DependencyInjection.Analyzers;

public enum CallSiteKind
{
    /// <summary>
    /// A user-defined factory method in the custom service provider with the <see cref="FactoryMethodAttribute{T}"/> attribute.
    /// </summary>
    Factory,
    /// <summary>
    /// A constructor call site.
    /// </summary>
    Constructor,
}