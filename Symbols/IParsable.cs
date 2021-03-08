namespace uld.definition.Symbols
{
    public interface IParsable
    {
        int MinimumNumberOfCharactersToParse { get; }

        bool TryParse(string str);
    }
}
