using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

    internal static string GetMethodSignature(this IMethodSymbol method)
    {
        var acc = method.DeclaredAccessibility.ToString().ToLower();

        var builder = new StringBuilder(acc);
        builder.Append(" ");
        builder.Append(method.ReturnType.GetFullNameWithGlobal());
        builder.Append(" ");
        builder.Append(method.Name);
        builder.Append("(");
        builder.Append(string.Join(", ", method.Parameters.Select(p => p.Type.GetFullNameWithGlobal())));
        builder.Append(");");
        return builder.ToString();
    }
    
    internal const string DependencyInjection_NAMESPACE = @"DanmakuEngine.DependencyInjection";

    internal const string PROVIDER_ATTRIBUTE_FULLNAME = @$"{DependencyInjection_NAMESPACE}.ServiceProviderAttribute";

    internal const string LifetimeBaseAttribute_FULLNAME = @$"{DependencyInjection_NAMESPACE}.LifetimeBaseAttribute";

    internal const string SingletonAttribute_FULLNAME = @$"{DependencyInjection_NAMESPACE}.SingletonAttribute";
    internal const string SingletonAttribute_GENERIC1_FULLNAME = @$"{SingletonAttribute_FULLNAME}`1";
    internal const string SingletonAttribute_GENERIC2_FULLNAME = @$"{SingletonAttribute_FULLNAME}`2";

    internal const string ScopedAttribute_FULLNAME = @$"{DependencyInjection_NAMESPACE}.ScopedAttribute";
    internal const string ScopedAttribute_GENERIC1_FULLNAME = @$"{ScopedAttribute_FULLNAME}`1";
    internal const string ScopedAttribute_GENERIC2_FULLNAME = @$"{ScopedAttribute_FULLNAME}`2";

    internal const string TransientAttribute_FULLNAME = @$"{DependencyInjection_NAMESPACE}.TransientAttribute";
    internal const string TransientAttribute_GENERIC1_FULLNAME = @$"{TransientAttribute_FULLNAME}`1";
    internal const string TransientAttribute_GENERIC2_FULLNAME = @$"{TransientAttribute_FULLNAME}`2";

    internal const string DependencyInjectionCreationAttribute_FULLNAME = @$"{DependencyInjection_NAMESPACE}.DependencyInjectionCreationAttribute";

    internal const string FactoryMethodAttribute_FULLNAME = @$"{DependencyInjection_NAMESPACE}.FactoryMethodAttribute";
}