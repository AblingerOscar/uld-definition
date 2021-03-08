using uld.definition.Symbols;
using System.Xml.Linq;

namespace uld.definition.Serialization
{
    public interface IInterfaceDeserializer
    {
        ISymbol DeserializeSymbol(XElement element);
        ITerminal DeserializeTerminalSymbol(XElement element);
        INonTerminal DeserializeNonTerminalSymbol(XElement element);
        IAction DeserializeAction(XElement element);
        IOneOf DeserializeOneOf(XElement element);

        ILanguageDefinition DeserializeLanguageDefinition(XElement element);
        IRule DeserializeRule(XElement element);
        CommentRule DeserializeCommentRule(XElement commentRule);
        CommentRules DeserializeCommentRules(XElement commentRules);
    }
}
