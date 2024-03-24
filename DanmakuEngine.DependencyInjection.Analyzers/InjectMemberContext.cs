using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;

namespace DanmakuEngine.DependencyInjection.Analyzers;

internal sealed class InjectMemberRecord : IEquatable<InjectMemberRecord>
{
    public InjectMemberType Type { get; }

    public GeneratorAttributeSyntaxContext Context { get; }

    public InjectMemberRecord(GeneratorAttributeSyntaxContext context)
    {
        Context = context;
        Symbol = context.TargetSymbol;
        
        if (Symbol is IFieldSymbol && context.TargetNode is FieldDeclarationSyntax)
        {
            Type = InjectMemberType.Field;
        }
        else if (Symbol is IPropertySymbol && context.TargetNode is PropertyDeclarationSyntax)
        {
            Type = InjectMemberType.Property;
        }
        else
        {
            throw new ArgumentException("MemberRecord must be a field or property");
        }
    }

    public MemberDeclarationSyntax Syntax => (MemberDeclarationSyntax)Context.TargetNode;
    public ISymbol Symbol { get; }

    public bool Equals(InjectMemberRecord other)
        => other is not null
            && other.Symbol.ToDisplayString() == Symbol.ToDisplayString();
}
