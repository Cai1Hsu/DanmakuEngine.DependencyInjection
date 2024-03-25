using Microsoft.CodeAnalysis;

namespace DanmakuEngine.DependencyInjection.Analyzers;

public interface ICanReport
{
    void Report(Diagnostic diagnostic);
}