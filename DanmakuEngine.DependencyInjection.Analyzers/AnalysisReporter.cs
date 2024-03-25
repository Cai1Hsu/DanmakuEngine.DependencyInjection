using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DanmakuEngine.DependencyInjection.Analyzers;

internal class AnalysisReporter
{
    private readonly Action<Diagnostic> _reportAction;

    internal AnalysisReporter(Action<Diagnostic> reportAction)
    {
        _reportAction = reportAction;
    }

    internal AnalysisReporter(SymbolAnalysisContext context)
        : this(context.ReportDiagnostic)
    {
    }

    internal AnalysisReporter(SourceProductionContext context)
        : this(context.ReportDiagnostic)
    {
    }

    internal void ReportDiagnostic(Diagnostic diagnostic)
        => _reportAction(diagnostic);
}
