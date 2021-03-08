using uld.definition.Serialization;
using uld.definition.Symbols;
using System.Collections.Generic;

namespace uld.definition
{
    public interface ILanguageDefinition : IXLinqSerializable
    {
        string LanguageId { get; }
        string LanguageFilePattern { get; }

        CommentRules CommentRules { get; }

        string[] StartRules { get; }
        IDictionary<string, IRule> Rules { get; }

        string[] VerifyAndGetErrors();
    }
}
