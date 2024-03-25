#pragma warning disable RS2008

namespace DanmakuEngine.DependencyInjection.Analyzers;

using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Text;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DependencyAnalyzer : DiagnosticAnalyzer
{

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create<DiagnosticDescriptor>(
        DiagnosticRules.DI_CTOR_MUST_BE_PUBLIC,
        DiagnosticRules.MULTIPLE_CTOR,
        DiagnosticRules.NO_VALID_CTOR,
        DiagnosticRules.MISSING_DEPENDENCY,
        DiagnosticRules.NO_PUBLIC_CTOR,
        DiagnosticRules.NO_MATCHED_CTOR,
        DiagnosticRules.IMPL_TYPE_MUST_BE_CLASS

#if DEBUG
        , DiagnosticRules.DEBUG_DIAGNOSTIC
#endif
    );

    public override void Initialize(AnalysisContext initContext)
    {
        // Do not analyze generated code
        initContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        initContext.EnableConcurrentExecution();

        initContext.RegisterSymbolAction(static ctx =>
        {
            // step1: select our service provider types.
            if (ctx.Symbol is not INamedTypeSymbol classSymbol)
                return;

            AnalyzeDependencyMap(new AnalysisReporter(ctx), classSymbol);
        }, SymbolKind.NamedType);
    }

    internal static void AnalyzeDependencyMap(AnalysisReporter reporter, INamedTypeSymbol classSymbol)
    {
        var attributes = classSymbol.GetAttributes();

        var providerAttributes = attributes.Where(
            static a => a.GetFullName() is SyntaxHelper.PROVIDER_ATTRIBUTE_FULLNAME);

        if (!providerAttributes.Any())
            return;

        var providerLocation = providerAttributes.First().GetSyntaxNode().GetLocation();

        // step2: get all the registration attributes andconvert to implmentation type list.
        IDictionary<INamedTypeSymbol, AttributeData> implTypes
            = new Dictionary<INamedTypeSymbol, AttributeData>(SymbolEqualityComparer.Default);

        ISet<INamedTypeSymbol> registered
            = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var a in attributes)
        {
            if (!a.AttributeClass!.IsGenericType)
                continue;

            var metadataName = a.AttributeClass!.GetFullMetadataName(false);

            var typeArgs = a.AttributeClass!.TypeArguments;

            ITypeSymbol? serviceType = null;
            ITypeSymbol implType = null!;

            if (metadataName is (
                SyntaxHelper.SingletonAttribute_GENERIC1_FULLNAME or
                SyntaxHelper.TransientAttribute_GENERIC1_FULLNAME or
                SyntaxHelper.ScopedAttribute_GENERIC1_FULLNAME))
            {
                if (typeArgs.Length != 1)
                    continue;

                implType = typeArgs[0];
            }
            else if (metadataName is (
                SyntaxHelper.SingletonAttribute_GENERIC2_FULLNAME or
                SyntaxHelper.TransientAttribute_GENERIC2_FULLNAME or
                SyntaxHelper.ScopedAttribute_GENERIC2_FULLNAME))
            {
                if (typeArgs.Length != 2)
                    continue;

                // The first type argument is the service type.
                // The second type argument is the implementation type.
                serviceType = typeArgs[0];
                implType = typeArgs[1];
            }
            else
            {
                continue;
            }

            // serviceType must be an interface or a class
            // In Microsoft.Extensions.DependencyInjection, it is allowed to register a class as a service.
            // And when you resolve the service, it will return the implementation type instead of the service type.
            // Kinda wired, i don't think anyone would use DI like this.
            if (serviceType is not null
                && serviceType.TypeKind != TypeKind.Interface
                && (serviceType.TypeKind != TypeKind.Class /*&& !serviceType.IsAbstract*/))
            {
                reporter.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticRules.SERVICE_TYPE_MUST_BE_INTERFACE_OR_CLASS,
                    a.GetSyntaxNode().GetLocation()
                ));

                continue;
            }

            if (implType.TypeKind != TypeKind.Class || implType.IsAbstract)
            {
                reporter.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticRules.IMPL_TYPE_MUST_BE_CLASS,
                    a.GetSyntaxNode().GetLocation()
                ));

                continue;
            }

            var namedType = (INamedTypeSymbol)implType;
            implTypes.Add(namedType, a);

            foreach (var t in typeArgs)
                registered.Add((INamedTypeSymbol)t);
        }

        IDictionary<INamedTypeSymbol, IMethodSymbol> allTypes
            = new Dictionary<INamedTypeSymbol, IMethodSymbol>(SymbolEqualityComparer.Default);

        // step3: analyze the dependencies.
        foreach (var implType in implTypes)
        {
            AnalyzeDependenciesRecursively(reporter, registered,
                implType.Key, implType.Value.GetSyntaxNode().GetLocation(),
                ref allTypes);
        }

#if DEBUG
        StringBuilder registeredTypes = new("Registered:\n");
        foreach (var t in registered)
        {
            registeredTypes.Append(t.GetFullName());
            registeredTypes.Append("\n");
        }

        reporter.ReportDiagnostic(Diagnostic.Create(
            DiagnosticRules.DEBUG_DIAGNOSTIC,
            providerLocation,
            registeredTypes.ToString()
        ));

        StringBuilder allTypesStr = new("All Types:\n");
        foreach (var t in allTypes)
        {
            allTypesStr.Append(t.Key.GetFullName());
            allTypesStr.Append(" -> ");
            allTypesStr.Append(t.Value.GetMethodSignature());
            allTypesStr.Append("\n");
        }

        reporter.ReportDiagnostic(Diagnostic.Create(
            DiagnosticRules.DEBUG_DIAGNOSTIC,
            providerLocation,
            allTypesStr.ToString()
        ));
#endif
    }

    internal static IMethodSymbol? SelectConstructor(
        AnalysisReporter reporter,
        ISet<INamedTypeSymbol> registered,
        INamedTypeSymbol type)
    {
        var pubCtors = type.Constructors.Where(static c => c.DeclaredAccessibility is Accessibility.Public);

        if (!pubCtors.Any())
        {
            foreach (var location in type.Locations)
            {
                reporter.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticRules.NO_PUBLIC_CTOR,
                    location,
                    type.Name
                ));
            }

            return null;
        }

        var markedPubCtors = pubCtors.Where(
            static c => c.GetAttributes().Any(
                static attr => attr.GetFullName() is SyntaxHelper.DependencyInjectionCreationAttribute_FULLNAME));

        if (markedPubCtors.Any())
        {
            int markedCount = markedPubCtors.Count();

            if (markedCount == 1)
                return markedPubCtors.First();

            // Should we report null or continue searching in markedPubCtors?
            return null;
        }

        int pubCount = pubCtors.Count();

        if (pubCount == 1)
            return pubCtors.First();

        // See if we have a paramless ctor.
        foreach (var pubCtor in pubCtors)
        {
            var paramless = !pubCtor.Parameters.Where(
                static p => !p.IsThis).Any();

            if (paramless)
                return pubCtor;
        }

        // step (4): Check if any ctor that matches all registered services.
        var matchedCtors = pubCtors.Where(
            pc => pc.Parameters.All(
                p => !p.IsThis && p.Type is INamedTypeSymbol 
                    && registered.Contains(p.Type, SymbolEqualityComparer.Default))
                // The dependency type must not be the same as the current type.
                && !pc.Parameters.Any(p => p.Type.GetFullMetadataName() != type.GetFullMetadataName()));

        if (!matchedCtors.Any())
        {
            foreach (var location in type.Locations)
            {
                reporter.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticRules.NO_MATCHED_CTOR,
                    location,
                    type.GetFullName()
                ));
            }

            return null;
        }

        if (matchedCtors.Count() == 1)
            return matchedCtors.First();

        // try select the first one with least parameters.
        var minCount = matchedCtors.Min(static mc => mc.Parameters.Count());
        var matched = matchedCtors.Where(mc => mc.Parameters.Count() == minCount);

        if (matched.Count() == 1)
            return matched.First();

        var ctor = matched.First();

        foreach (var location in ctor.Locations)
        {
            reporter.ReportDiagnostic(Diagnostic.Create(
                DiagnosticRules.MULTIPLE_CTOR,
                location,
                ctor.GetMethodSignature()
            ));
        }

        return ctor;
    }

    internal static void AnalyzeDependenciesRecursively(
            AnalysisReporter reporter,
            ISet<INamedTypeSymbol> registered,
            INamedTypeSymbol baseDep,
            Location attributeLocation,
            ref IDictionary<INamedTypeSymbol, IMethodSymbol> allTypes
        )
    {
        var ctor = SelectConstructor(reporter, registered, baseDep);

        // Should have reported this before.
        if (ctor is null)
            return;

        var deps = ctor.Parameters.Where(static p => !p.IsThis)
                                  .Select(static p => (p.Type as INamedTypeSymbol)!)
                                  .Where(static p => p is not null)
                                  .ToImmutableArray();

        foreach (var dep in deps)
        {
            if (!registered.Contains(dep))
            {
                reporter.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticRules.MISSING_DEPENDENCY,
                    attributeLocation,
                    dep.GetFullName(),
                    baseDep.GetFullName()
                ));

                continue;
            }

            AnalyzeDependenciesRecursively(
                reporter,
                registered,
                dep,
                attributeLocation,
                ref allTypes
            );
        }

        allTypes.Add(baseDep, ctor);
    }
}
