using uld.definition.Serialization;
using System.Collections.Immutable;

namespace uld.definition.Symbols
{
    public interface IRule : IXLinqSerializable
    {
        string Name { get; }

        IImmutableList<ISymbol> Symbols { get; }
    }
}
