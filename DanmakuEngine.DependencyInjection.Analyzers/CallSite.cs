using Microsoft.CodeAnalysis;

namespace DanmakuEngine.DependencyInjection.Analyzers;

public class CallSite(
    CallSiteKind kind, INamedTypeSymbol type, IMethodSymbol method)
{
    public CallSiteKind Kind { get; } = kind;
    public INamedTypeSymbol Type { get; } = type;
    public IMethodSymbol MethodSymbol { get; } = method;
}
