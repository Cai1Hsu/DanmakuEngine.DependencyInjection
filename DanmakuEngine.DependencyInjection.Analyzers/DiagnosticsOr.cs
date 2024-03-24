#nullable enable

using Microsoft.CodeAnalysis;

namespace DanmakuEngine.DependencyInjection.Analyzers;

public class DiagnosticsOr<TValue>
{
    private readonly TValue? _value;
    private readonly DiagnosticDescriptor? _diagnostic;
    private readonly Location? _location;

    public bool IsValue => _value is not null;
    public bool IsDiag => _diagnostic is not null;

    public TValue? Value => _value;
    public DiagnosticDescriptor? Diag => _diagnostic;
    public Location? Location => _location;

    internal DiagnosticsOr(DiagnosticDescriptor diagnostic, Location location)
    {
        _diagnostic = diagnostic;
        _location = location;
    }

    internal DiagnosticsOr(TValue value)
    {
        _value = value;
    }
}

public static class DiagnosticsOr
{
    public static DiagnosticsOr<TValue> FromValue<TValue>(TValue value)
        => new(value);

    public static DiagnosticsOr<TValue> FromDiag<TValue>(DiagnosticDescriptor diagnostic, Location location)
        => new(diagnostic, location);
}

