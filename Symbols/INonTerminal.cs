namespace uld.definition.Symbols
{
    public interface INonTerminal : ISymbol
    {
        string ReferencedRule { get; }
    }
}
