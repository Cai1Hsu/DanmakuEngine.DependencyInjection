#pragma warning disable RS2008 

using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DanmakuEngine.DependencyInjection.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeUsageAnalyzer : DiagnosticAnalyzer
{
#if DEBUG
    // used to debug the analyzer.
    private static readonly DiagnosticDescriptor DEBUG = new(
        "DEDI0000",
        "DEBUG: {0}",
        "DEBUG: {0}",
        "DependencyInjection",
        DiagnosticSeverity.Warning,
        true
    );
#endif
    private static readonly DiagnosticDescriptor LIFETIME_MARKED_NOT_ON_PROVIDER = new(
        "DEDI0001",
        "Lifetime attribute must be marked on a service provider class",
        "Lifetime attribute must be marked on a service provider class",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor DO_NOT_USE_NON_GENERIC = new(
        "DEDI0002",
        "Do not use the non-generic attribute, use the generic attribute instead",
        "Do not use the non-generic attribute, use the generic attribute instead",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor DO_NOT_USE_BASE_ATTRIBUTE = new(
        "DEDI0003",
        "Do not use the base attribute, use the derived attribute instead",
        "Do not use the base attribute, use the derived attribute instead",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor ONLY_ONE_CTOR_ALLOWED = new(
        "DEDI0004",
        "Only one constructor can be used for dependency injection creation",
        "Only one constructor can be used for dependency injection creation",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor DI_CTOR_MUST_BE_PUBLIC = new(
        "DEDI0005",
        "Dependency injection constructor must be public",
        "Dependency injection constructor must be public",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor MULTIPLE_CTOR = new(
        "DEDI0006",
        "Multiple constructors detected, please mark the constructor to use for dependency injection with [DependencyInjectionCreation] attribute",
        "Multiple constructors detected, please mark the constructor to use for dependency injection with [DependencyInjectionCreation] attribute",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor NO_VALID_CTOR = new(
        "DEDI0007",
        "No valid constructor found for dependency injection",
        "No valid constructor found for dependency injection",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor MISSING_DEPENDENCY = new(
        "DEDI0008",
        "Missing dependency {0}",
        "Missing dependency {0}",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor NO_PUBLIC_CTOR = new(
        "DEDI0009",
        "No public constructor found for {0}",
        "No public constructor found for {0}",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    private static readonly DiagnosticDescriptor IMPL_TYPE_MUST_BE_CLASS = new(
        "DEDI0010",
        "Implementation type must be a class and can not be abstract",
        "Implementation type must be a class and can not be abstract",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        LIFETIME_MARKED_NOT_ON_PROVIDER,
        DO_NOT_USE_NON_GENERIC,
        DO_NOT_USE_BASE_ATTRIBUTE,
        ONLY_ONE_CTOR_ALLOWED,
        DI_CTOR_MUST_BE_PUBLIC,
        MULTIPLE_CTOR,
        NO_VALID_CTOR,
        MISSING_DEPENDENCY,
        NO_PUBLIC_CTOR,
        IMPL_TYPE_MUST_BE_CLASS

#if DEBUG
        , DEBUG
#endif
    );

    private const string DependencyInjection_NAMESPACE = @"DanmakuEngine.DependencyInjection";

    private const string PROVIDER_ATTRIBUTE_FULLNAME = @$"{DependencyInjection_NAMESPACE}.ServiceProviderAttribute";

    private const string LifetimeBaseAttribute_FULLNAME = @$"{DependencyInjection_NAMESPACE}.LifetimeBaseAttribute";

    private const string SingletonAttribute_FULLNAME = @$"{DependencyInjection_NAMESPACE}.SingletonAttribute";
    private const string SingletonAttribute_GENERIC1_FULLNAME = @$"{SingletonAttribute_FULLNAME}`1";
    private const string SingletonAttribute_GENERIC2_FULLNAME = @$"{SingletonAttribute_FULLNAME}`2";

    private const string ScopedAttribute_FULLNAME = @$"{DependencyInjection_NAMESPACE}.ScopedAttribute";
    private const string ScopedAttribute_GENERIC1_FULLNAME = @$"{ScopedAttribute_FULLNAME}`1";
    private const string ScopedAttribute_GENERIC2_FULLNAME = @$"{ScopedAttribute_FULLNAME}`2";

    private const string TransientAttribute_FULLNAME = @$"{DependencyInjection_NAMESPACE}.TransientAttribute";
    private const string TransientAttribute_GENERIC1_FULLNAME = @$"{TransientAttribute_FULLNAME}`1";
    private const string TransientAttribute_GENERIC2_FULLNAME = @$"{TransientAttribute_FULLNAME}`2";

    private const string DependencyInjectionCreationAttribute_FULLNAME = @$"{DependencyInjection_NAMESPACE}.DependencyInjectionCreationAttribute";

    public override void Initialize(AnalysisContext initContext)
    {
        // Do not analyze generated code
        initContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        initContext.EnableConcurrentExecution();

        initContext.RegisterSymbolAction(AnalyzeClassAttributeUsage, SymbolKind.NamedType);

        // Build a map to see if all implementation types and dependency(and dependency of dependency..) types are registered and can has a valid constructor.
        // This means that in the end, any type registered can be created with types that has a parameterless constructor.(and it must be public)

        // We only do this at build time because this is a ratherexpensive operation.
        // initContext.RegisterCompilationStartAction(RegisterComspilationStart);
        initContext.RegisterSymbolAction(AnalyzeDependencyMap, SymbolKind.NamedType);

        initContext.RegisterCompilationStartAction(static ctx =>
        {
        });
    }

    private static void AnalyzeDependencyMap(SymbolAnalysisContext context)
    {
        // step1: select our service provider types.
        if (context.Symbol is not INamedTypeSymbol classSymbol)
            return;

        var attributes = classSymbol.GetAttributes();

        var providerAttributes = attributes.Where(static a => a.GetFullName() == PROVIDER_ATTRIBUTE_FULLNAME);

        if (!providerAttributes.Any())
            return;

        var provider = providerAttributes.First();

        // step2: get all the registration attributes andconvert to implmentation type list.
        IList<INamedTypeSymbol> implTypes = [];
        HashSet<ITypeSymbol> registered = new(SymbolEqualityComparer.Default);

        foreach (var a in attributes)
        {
            if (!a.AttributeClass!.IsGenericType)
                continue;

            var metadataName = a.AttributeClass!.GetFullMetadataName(false);

            var typeArgs = a.AttributeClass!.TypeArguments;

            ITypeSymbol? serviceType = null;
            ITypeSymbol implType = null!;

            if (metadataName is (
                SingletonAttribute_GENERIC1_FULLNAME or
                TransientAttribute_GENERIC1_FULLNAME or
                ScopedAttribute_GENERIC1_FULLNAME))
            {
                if (typeArgs.Length != 1)
                    continue;

                implType = typeArgs[0];
            }
            else if (metadataName is (
                SingletonAttribute_GENERIC2_FULLNAME or
                TransientAttribute_GENERIC2_FULLNAME or
                ScopedAttribute_GENERIC2_FULLNAME))
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

            if (implType.TypeKind != TypeKind.Class || implType.IsAbstract)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    IMPL_TYPE_MUST_BE_CLASS,
                    a.GetSyntaxNode().GetLocation()
                ));

                continue;
            }

            var namedType = (INamedTypeSymbol)implType;
            implTypes.Add(namedType);

            foreach (var t in typeArgs)
                registered.Add(t);
        }

        // step3: get all the dependency(and dependency of dependency...) types.
        // IList<ITypeSymbol> depTypes = [];

        // this should be done recursively until we meet a type that has no dependency(has a parameterless constructor).
        // we should also check if the constructor is public.
        // and check if all of them are registered.

        // then, we check if all dependency types has a valid constructor.

        HashSet<INamedTypeSymbol> allDeps = new(SymbolEqualityComparer.Default);

        foreach (var t in implTypes)
            allDeps.Add(t);

        AnalyzeDependencies(implTypes, ref allDeps, context);

        foreach (var dep in allDeps)
        {
            // Analyze missing registration.
            if (!registered.Contains(dep))
            {
                foreach (var location in dep.Locations.FilterInSource())
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        MISSING_DEPENDENCY,
                        location,
                        dep.Name
                    ));
                }

                continue;
            }

            // Analyze constructor.
            var pubCotor = dep.Constructors.Where(c => c.DeclaredAccessibility == Accessibility.Public);
            if (!pubCotor.Any())
            {
                foreach (var location in dep.Locations.FilterInSource())
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        NO_PUBLIC_CTOR,
                        location
                    ));
                }

                continue;
            }

            if (pubCotor.Count() > 1
                && !pubCotor.Any(
                    static c => c.GetAttributes().Any(
                        static a => a.GetFullName() == DependencyInjectionCreationAttribute_FULLNAME)))
            {
                foreach (var c in pubCotor)
                {
                    foreach (var location in c.Locations.FilterInSource())
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            MULTIPLE_CTOR,
                            location
                        ));
                    }
                }

                continue;
            }
        }
    }

    private static void AnalyzeDependencies(
        IEnumerable<INamedTypeSymbol> types,
        ref HashSet<INamedTypeSymbol> allDeps,
        SymbolAnalysisContext context)
    {
        foreach (var t in types)
        {
            var pubCotor = t.Constructors.Where(c => c.DeclaredAccessibility == Accessibility.Public);

            IMethodSymbol? ctor = null;
            foreach (var c in pubCotor)
            {
                if (ctor == null)
                    ctor = c;

                // The one with DependencyInjectionCreationAttribute is the one we want.
                if (c.GetAttributes().Any(static a => a.GetFullName() == DependencyInjectionCreationAttribute_FULLNAME))
                    ctor = c;
            }

            // report if no valid constructor found.
            if (ctor is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    NO_VALID_CTOR,
                    t.Locations.FilterInSource().First()
                ));

                continue;
            }

            var depTypes = ctor.Parameters.Where(p => !p.IsThis)
                                          .Select(p => (p.Type as INamedTypeSymbol)!)
                                          .Where(p => p is not null);

            foreach (var dep in depTypes)
                allDeps.Add(dep!);

            // make deps a base type and analyze their dependencies.
            AnalyzeDependencies(depTypes, ref allDeps, context);
        }
    }

    private static void AnalyzeClassAttributeUsage(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol classSymbol)
            return;

        var classAttributes = classSymbol.GetAttributes();


        // These are not allowed to use.
        // We want user to use the generic version of the attribute.
        // The generic version constraints the implementation type to be a class and the service type to be the same as the implementation type or a base class of the implementation type.
        var banedAttributes = classAttributes.Where(static a => a.GetFullName() is (
            // TODO: distinguish with INamedTypeSymbol.IsGenericType
            SingletonAttribute_FULLNAME or
            TransientAttribute_FULLNAME or
            ScopedAttribute_FULLNAME));

        foreach (var baned in banedAttributes)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DO_NOT_USE_NON_GENERIC,
                baned.GetSyntaxNode().GetLocation()
            ));
        }

        var isProviderType = classAttributes.Any(a => a.GetFullName() == PROVIDER_ATTRIBUTE_FULLNAME);

        if (!isProviderType)
        {
            var lifetimeAttributes = classAttributes.Where(static a => a.AttributeClass!.GetFullMetadataName(false) is (
                SingletonAttribute_GENERIC1_FULLNAME or
                SingletonAttribute_GENERIC2_FULLNAME or
                TransientAttribute_GENERIC1_FULLNAME or
                TransientAttribute_GENERIC2_FULLNAME or
                ScopedAttribute_GENERIC1_FULLNAME or
                ScopedAttribute_GENERIC2_FULLNAME));

            foreach (var lifetime in classAttributes)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    LIFETIME_MARKED_NOT_ON_PROVIDER,
                    lifetime.GetSyntaxNode().GetLocation()
                ));
            }
        }

        var baseAttributes = classAttributes.Where(static a => a.GetFullName() is LifetimeBaseAttribute_FULLNAME);
        foreach (var baseAttribute in baseAttributes)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DO_NOT_USE_BASE_ATTRIBUTE,
                baseAttribute.GetSyntaxNode().GetLocation()
            ));
        }

        var attributedCtors = classSymbol.Constructors.Where(
            static c => c.GetAttributes().Any(static a => a.GetFullName() == DependencyInjectionCreationAttribute_FULLNAME));

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
                        ONLY_ONE_CTOR_ALLOWED,
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
                        DI_CTOR_MUST_BE_PUBLIC,
                        location
                    ));
                }
            }
        }
    }
}
