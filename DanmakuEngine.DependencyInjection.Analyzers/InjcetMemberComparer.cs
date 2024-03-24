using System.Collections.Generic;

namespace DanmakuEngine.DependencyInjection.Analyzers;

// do not use this for pipeline, as the attributes may change but this comaprer will not be updated
internal sealed class InjectMemberNameComparer : IEqualityComparer<InjectMemberRecord>
{
    public bool Equals(InjectMemberRecord x, InjectMemberRecord y)
        => x is not null && y is not null
            && x.Symbol.ToDisplayString() == y.Symbol.ToDisplayString();

    public int GetHashCode(InjectMemberRecord obj)
        => obj.Symbol.ToDisplayString().GetHashCode();
}
