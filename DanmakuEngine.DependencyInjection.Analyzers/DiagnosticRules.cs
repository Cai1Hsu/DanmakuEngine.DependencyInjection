#pragma warning disable RS2008

using Microsoft.CodeAnalysis;

namespace DanmakuEngine.DependencyInjection.Analyzers;

internal static class DiagnosticRules
{
#if DEBUG
    internal static readonly DiagnosticDescriptor DEBUG_DIAGNOSTIC = new(
        "DEDI0000",
        "{0}",
        "{0}",
        "DependencyInjection",
        DiagnosticSeverity.Warning,
        true
    );
#endif

    internal static readonly DiagnosticDescriptor LIFETIME_MARKED_NOT_ON_PROVIDER = new(
        "DEDI0001",
        "Lifetime attribute must be marked on a service provider class",
        "Lifetime attribute must be marked on a service provider class",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor DO_NOT_USE_NON_GENERIC = new(
        "DEDI0002",
        "Do not use the non-generic attribute, use the generic attribute instead",
        "Do not use the non-generic attribute, use the generic attribute instead",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor DO_NOT_USE_BASE_ATTRIBUTE = new(
        "DEDI0003",
        "Do not use the base attribute, use the derived attribute instead",
        "Do not use the base attribute, use the derived attribute instead",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor ONLY_ONE_CTOR_ALLOWED = new(
        "DEDI0004",
        "Only one constructor can be used for dependency injection creation",
        "Only one constructor can be used for dependency injection creation",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor DI_CTOR_MUST_BE_PUBLIC = new(
        "DEDI0005",
        "Dependency injection constructor must be public",
        "Dependency injection constructor must be public",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor MULTIPLE_CTOR = new(
        "DEDI0006",
        "Multiple constructors detected, only '{0}' will be used, please mark the constructor to use for dependency injection with [DependencyInjectionCreation] attribute",
        "Multiple constructors detected, only '{0}' will be used, please mark the constructor to use for dependency injection with [DependencyInjectionCreation] attribute",
        "DependencyInjection",
        DiagnosticSeverity.Warning,
        true
    );

    internal static readonly DiagnosticDescriptor NO_VALID_CTOR = new(
        "DEDI0007",
        "No valid constructor found for dependency injection",
        "No valid constructor found for dependency injection",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor MISSING_DEPENDENCY = new(
        "DEDI0008",
        "Missing dependency {0}, required by {1}",
        "Missing dependency {0}, required by {1}",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor NO_PUBLIC_CTOR = new(
        "DEDI0009",
        "No public constructor found for {0}",
        "No public constructor found for {0}",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor NO_MATCHED_CTOR = new(
        "DEDI0010",
        "No valid constructor found for {0}, please either register all dependencies or mark the constructor with [DependencyInjectionCreation] attribute",
        "No valid constructor found for {0}, please either register all dependencies or mark the constructor with [DependencyInjectionCreation] attribute",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor SERVICE_TYPE_MUST_BE_INTERFACE_OR_CLASS = new(
        "DEDI0011",
        "Service type must be an interface or a class",
        "Service type must be an interface or a class",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor IMPL_TYPE_MUST_BE_CLASS = new(
        "DEDI0012",
        "Implementation type must be a class and can not be abstract",
        "Implementation type must be a class and can not be abstract",
        "DependencyInjection",
        DiagnosticSeverity.Error,
        true
    );
}
