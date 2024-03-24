#pragma warning disable RS2008

using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DanmakuEngine.DependencyInjection.Analyzers;

[Generator]
public class ServiceProviderGenerator : IIncrementalGenerator
{
    private const string ServiceProviderAttribute_FULLNAME = @"DanmakuEngine.DependencyInjection.ServiceProviderAttribute";
    private const string InjectAttribute_FULLNAME = @"DanmakuEngine.DependencyInjection.InjectAttribute";

    private const string ServiceProviderBase_FULLNAME_WITH_GLOBAL = @"global::DanmakuEngine.DependencyInjection.ServiceProviderBase";
    // FIXME: it should be `object`
    private const string SystemObject_FULLNAME_WITH_GLOBAL = @"global::System.Object";

    private static readonly IEqualityComparer<InjectMemberRecord> INJECT_MEMBER_NAME_COMPARER = new InjectMemberNameComparer();

    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        // classes decorated with `[ServiceProvider]`
        var providerAttributedClasses = initContext.SyntaxProvider
            .ForAttributeWithMetadataName(
                ServiceProviderAttribute_FULLNAME,
                static (node, _) => node.IsKind(SyntaxKind.ClassDeclaration),
                static (ctx, _) => ctx.TargetNode is ClassDeclarationSyntax classSyntax
                    && ctx.TargetSymbol is INamedTypeSymbol typeSymbol
                        ? new ClassRecord(classSyntax, typeSymbol)
                        : null!)
            .Where(c => c is not null);

        // HACK: Remove me
        initContext.RegisterSourceOutput(providerAttributedClasses, static (ctx, @class) =>
        {
            ctx.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                "DESG0000",
                "Service provider class found",
                "Service provider class found",
                "DanmakuEngine.DependencyInjection",
                DiagnosticSeverity.Warning,
                true),
                @class.Symbol.Locations.First()
            ));
        });

        // Validate if the user-defined service provider can be generated
        var providerClassWithDiagnostics = providerAttributedClasses.Select(static (classRecord, _) =>
        {
            var valid = TryGetDiagnosticForCustomProvider(classRecord!, out var diag, out var location);

            return valid ? DiagnosticsOr.FromValue<ClassRecord>(classRecord!)
                         : DiagnosticsOr.FromDiag<ClassRecord>(diag, location);
        });

        var providerClass = FilterAndReportError(initContext, providerClassWithDiagnostics);

        // Find out dependencies of the service classes
        // We have to generate factory methods for them
        var dependenciesToGenerateFactoryMethods = initContext.SyntaxProvider
            .ForAttributeWithMetadataName(
                InjectAttribute_FULLNAME,
                static (node, _) => node.IsKind(SyntaxKind.PropertyDeclaration)
                                 || node.IsKind(SyntaxKind.FieldDeclaration),
                // we have to keep the context as we need the semantic model
                static (ctx, _) => new InjectMemberRecord(ctx))
            .Collect()
            // Make sure we have unique members, only the fully qualified name is enough to compare
            .SelectMany(static (members, _) => members.Distinct(INJECT_MEMBER_NAME_COMPARER));

        // Get the semantic model as we need it to get the symbol with the syntax node of the dependencies

        var servicesToGenerateFactoryMethods = dependenciesToGenerateFactoryMethods
            .Select(static (member, _) =>
            {
                // TODO: Determine the return type.
                return member.Type switch
                {
                    InjectMemberType.Field => member.Syntax.Parent as ClassDeclarationSyntax,
                    InjectMemberType.Property => member.Syntax.Parent as ClassDeclarationSyntax,
                    // may be we should return null and filter out later
                    _ => throw new InvalidOperationException("Invalid member type")
                };
            });

        // TODO:
        // Get the type port of the dependencies and ignores interfaces as they can not be created with new statement
        // interfaces are created by the service provider with actual implementations

        // TODO:
        // Combine the service classes and dependencies

        // TODO:
        // Check that if the classes have public constructors
        // or if they are partial so we can generate a public static factory method for them first(This may not be supported.)

        // TODO
        // For those inteferces that are mapped to a concrete class, we should analyzes all `Add<TInterface, IImpl>` calls to generate factory methods for them

        // As for those we cant generate factory methods for, we should report diagnostics and fallback to use reflection

        // and if they are abstract

        // TODO:
        // Generate code for the service provider class with the factory methods
        // initContext.RegisterSourceOutput(classToGenerate, Execute);
    }

    private static void Execute(SourceProductionContext ctx, ClassRecord classRecord)
    {
#if DEBUG
        Debugger.Break();
#endif
    }

    private static IncrementalValuesProvider<ClassRecord> FilterAndReportError(IncrementalGeneratorInitializationContext initContext,
                                                                               IncrementalValuesProvider<DiagnosticsOr<ClassRecord>> diagOrClasses)
    {
        // TODO: We should make diags a collection so that all diagnostics can be reported at once

        var diags = diagOrClasses.Where(static doc => doc.IsDiag)
                                 .Select(static (doc, _) => (doc.Diag!, doc.Location!));

        // reports the diagnostics
        initContext.RegisterSourceOutput(diags, static (ctx, diagInfo) =>
        {
            var (diagnostics, location) = diagInfo;

            ctx.ReportDiagnostic(Diagnostic.Create(diagnostics, location));
        });

        return diagOrClasses.Where(static doc => doc.IsValue)
                            .Select(static (doc, _) => doc.Value!);
    }

    private static readonly DiagnosticDescriptor MUST_NOT_DECLARED_IN_ANOTHER_CLASS = new(
            "DESG0001",
            "The service provider class must not be declared within another class",
            "The service provider class must not be declared within another class",
            "Correction",
            DiagnosticSeverity.Error,
            true
        );

    private static readonly DiagnosticDescriptor MUST_NOT_BE_STATIC = new(
            "DESG0002",
            "The service provider class must not be static",
            "The service provider class must not be static",
            "Correction",
            DiagnosticSeverity.Error,
            true
        );

    private static readonly DiagnosticDescriptor MUST_BE_PARTIAL = new(
            "DESG0003",
            "The service provider class must be partial",
            "The service provider class must be partial",
            "Correction",
            DiagnosticSeverity.Error,
            true
        );

    private static readonly DiagnosticDescriptor MUST_NOT_HAVE_CONSTRUCTORS = new(
            "DESG0004",
            "The service provider class must not contain any constructors",
            "The service provider class must not contain any constructors",
            "Correction",
            DiagnosticSeverity.Error,
            true
        );

    private static readonly DiagnosticDescriptor BASE_TYPE_ISSUE = new(
            "DESG0005",
            "The service provider class must derive from ServiceProviderBase or by default from System.Object",
            "The service provider class must derive from ServiceProviderBase or by default from System.Object",
            "Correction",
            DiagnosticSeverity.Error,
            true
        );

    private static bool TryGetDiagnosticForCustomProvider(ClassRecord classRecord, out DiagnosticDescriptor diag, out Location location)
    {
        // The class must not be declared in another class.
        if (classRecord.Syntax.Parent is TypeDeclarationSyntax)
        {
            diag = MUST_NOT_DECLARED_IN_ANOTHER_CLASS;

            location = classRecord.Syntax.GetLocation();
            return false;
        }

        // The class must not be static.
        var staticModifier = classRecord.Syntax.Modifiers.Where(m => m.IsKind(SyntaxKind.StaticKeyword));
        if (staticModifier.Any())
        {
            diag = MUST_NOT_BE_STATIC;

            location = staticModifier.First().GetLocation();
            return false;
        }

        // The class must be partial.
        if (!classRecord.Syntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            diag = MUST_BE_PARTIAL;

            location = classRecord.Syntax.GetLocation();
            return false;
        }

        // The class must not contains any constructor.
        var constructors = classRecord.Syntax.Members.Where(m => m.IsKind(SyntaxKind.ConstructorDeclaration));
        if (constructors.Any())
        {
            diag = MUST_NOT_HAVE_CONSTRUCTORS;

            // The user may define many constructors, but we'll only report the first location since the developer must remove them all to compile.
            location = constructors.First().GetLocation();
            return false;
        }

        // The class must derive from DanmakuEngine.DependencyInjection.ServiceProviderBase or by default from System.Object.
        if (classRecord.Symbol.BaseType is not null &&
            classRecord.Symbol.BaseType.ToDisplayString(SyntaxHelper.DisplayFormat_FullNameWithGlobal)
                is not (SystemObject_FULLNAME_WITH_GLOBAL or ServiceProviderBase_FULLNAME_WITH_GLOBAL))
        {
            diag = BASE_TYPE_ISSUE;

            location = classRecord.Syntax.BaseList!.GetLocation();
            return false;
        }

        // The class is safe to generate code for.
        diag = null!;
        location = null!;

        return true;
    }
}
