using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace DanmakuEngine.DependencyInjection.Analyzers;

internal sealed class ClassRecord(ClassDeclarationSyntax classSyntax, INamedTypeSymbol typeSymbol)
{
    public ClassDeclarationSyntax Syntax { get; } = classSyntax;
    public INamedTypeSymbol Symbol { get; } = typeSymbol;
}
