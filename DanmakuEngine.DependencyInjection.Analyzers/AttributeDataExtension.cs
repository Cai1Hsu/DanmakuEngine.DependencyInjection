using System.Text;
using Microsoft.CodeAnalysis;

namespace DanmakuEngine.DependencyInjection.Analyzers;

public static class AttributeDataExtension
{
    public static string GetMinimalQualifiedName(this AttributeData attributeData)
        => attributeData.AttributeClass!.GetMinimalQualifiedName();

    public static string GetFullName(this AttributeData attributeData)
        => attributeData.AttributeClass!.ToDisplayString(SyntaxHelper.DisplayFormat_FullNameWithoutGlobal);

    public static SyntaxNode GetSyntaxNode(this AttributeData attributeData)
        => attributeData.ApplicationSyntaxReference!.GetSyntax();

    public static string GetFullyQualifiedName(this ISymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static string GetMinimalQualifiedName(this ISymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    public static string GetFullMetadataName(this ISymbol symbol, bool withGlobal = true)
    {
        StringBuilder sb = new(withGlobal ? "global::" : string.Empty);

        var ns = symbol.ContainingNamespace;
        if (ns is not null)
        {
            sb.Append(ns.GetFullName());
            sb.Append('.');
        }

        sb.Append(symbol.MetadataName);

        return sb.ToString();
    }
}