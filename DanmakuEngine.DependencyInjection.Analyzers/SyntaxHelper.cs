using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DanmakuEngine.DependencyInjection.Analyzers;

internal static class SyntaxHelper
{
    // Display format for the fully qualified name with global namespace
    internal static readonly SymbolDisplayFormat DisplayFormat_FullNameWithGlobal = new(
        SymbolDisplayGlobalNamespaceStyle.Included,
        // With 'global::'
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        // This includes `<T>` where T is the actual type parameter like `System.String`
        SymbolDisplayGenericsOptions.IncludeTypeParameters
    );

    // Display format for the fully qualified name without global namespace
    internal static readonly SymbolDisplayFormat DisplayFormat_FullNameWithoutGlobal = new(
        SymbolDisplayGlobalNamespaceStyle.Omitted,
        // Without 'global::'
        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        SymbolDisplayGenericsOptions.IncludeTypeParameters
    );

    internal static string GetFullName(this ISymbol symbol)
        => symbol.ToDisplayString(DisplayFormat_FullNameWithoutGlobal);

    internal static string GetFullNameWithGlobal(this ISymbol symbol)
        => symbol.ToDisplayString(DisplayFormat_FullNameWithGlobal);

    internal static IEnumerable<Location> FilterInSource(this IEnumerable<Location> locations)
        => locations.Where(loc => loc.IsInSource);
}