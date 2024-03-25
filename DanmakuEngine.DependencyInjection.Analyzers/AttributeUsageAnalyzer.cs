#pragma warning disable RS2008 

using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DanmakuEngine.DependencyInjection.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeUsageAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        DiagnosticRules.LIFETIME_MARKED_NOT_ON_PROVIDER,
        DiagnosticRules.DO_NOT_USE_NON_GENERIC,
        DiagnosticRules.DO_NOT_USE_BASE_ATTRIBUTE,
        DiagnosticRules.ONLY_ONE_CTOR_ALLOWED,
        DiagnosticRules.DI_CTOR_MUST_BE_PUBLIC

#if DEBUG
        , DiagnosticRules.DEBUG_DIAGNOSTIC
#endif
    );

    public override void Initialize(AnalysisContext initContext)
    {
        // Do not analyze generated code
        initContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        initContext.EnableConcurrentExecution();

        initContext.RegisterSymbolAction(AnalyzeAttributeUsage, SymbolKind.NamedType);
    }

    private static void AnalyzeAttributeUsage(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol classSymbol)
            return;

        var classAttributes = classSymbol.GetAttributes();

        // These are not allowed to use.
        // We want user to use the generic version of the attribute.
        // The generic version constraints the implementation type to be a class and the service type to be the same as the implementation type or a base class of the implementation type.
        var banedAttributes = classAttributes.Where(static a => a.GetFullName() is (
            // TODO: distinguish with INamedTypeSymbol.IsGenericType
            SyntaxHelper.SingletonAttribute_FULLNAME or
            SyntaxHelper.TransientAttribute_FULLNAME or
            SyntaxHelper.ScopedAttribute_FULLNAME));

        foreach (var baned in banedAttributes)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticRules.DO_NOT_USE_NON_GENERIC,
                baned.GetSyntaxNode().GetLocation()
            ));
        }

        var isProviderType = classAttributes.Any(
            static a => a.GetFullName() is SyntaxHelper.PROVIDER_ATTRIBUTE_FULLNAME);

        if (!isProviderType)
        {
            var lifetimeAttributes = classAttributes.Where(static a => a.AttributeClass!.GetFullMetadataName(false) is (
                SyntaxHelper.SingletonAttribute_GENERIC1_FULLNAME or
                SyntaxHelper.SingletonAttribute_GENERIC2_FULLNAME or
                SyntaxHelper.TransientAttribute_GENERIC1_FULLNAME or
                SyntaxHelper.TransientAttribute_GENERIC2_FULLNAME or
                SyntaxHelper.ScopedAttribute_GENERIC1_FULLNAME or
                SyntaxHelper.ScopedAttribute_GENERIC2_FULLNAME));

            foreach (var lifetime in classAttributes)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticRules.LIFETIME_MARKED_NOT_ON_PROVIDER,
                    lifetime.GetSyntaxNode().GetLocation()
                ));
            }
        }

        var baseAttributes = classAttributes.Where(
            static a => a.GetFullName() is SyntaxHelper.LifetimeBaseAttribute_FULLNAME);
        foreach (var baseAttribute in baseAttributes)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticRules.DO_NOT_USE_BASE_ATTRIBUTE,
                baseAttribute.GetSyntaxNode().GetLocation()
            ));
        }

        var attributedCtors = classSymbol.Constructors.Where(
            static c => c.GetAttributes().Any(
                static a => a.GetFullName() is SyntaxHelper.DependencyInjectionCreationAttribute_FULLNAME));

        var count = 0;
        foreach (var ctor in attributedCtors)
        {
            count++;

            // check count.
            if (count > 1)
            {
                foreach (var location in ctor.Locations.FilterInSource())
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticRules.ONLY_ONE_CTOR_ALLOWED,
                        location
                    ));
                }
            }

            // check accessibility.
            if (ctor.DeclaredAccessibility != Accessibility.Public)
            {
                foreach (var location in ctor.Locations.FilterInSource())
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticRules.DI_CTOR_MUST_BE_PUBLIC,
                        location
                    ));
                }
            }
        }
    }
}
