using autosupport_lsp_server.Serialization;
using autosupport_lsp_server.Symbols;
using System.Collections.Generic;
using System.Xml.Linq;

namespace autosupport_lsp_server
{
    public interface IAutosupportLanguageDefinition : IXLinqSerializable
    {
        string LanguageId { get; }
        string LanguageFilePattern { get; }

        CommentRules CommentRules { get; }

        string[] StartRules { get; }
        IDictionary<string, IRule> Rules { get; }

        string[] VerifyAndGetErrors();
    }
}
