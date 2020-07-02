using System.Collections.Immutable;

namespace autosupport_lsp_server
{
    public static class Constants
    {
        public static char NewLine = '\n';

        public readonly static ImmutableArray<string> ValidKinds = ImmutableArray.Create(new[]
            {
                "Text",
                "Method",
                "Function",
                "Constructor",
                "Field",
                "Variable",
                "Class",
                "Interface",
                "Module",
                "Property",
                "Unit",
                "Value",
                "Enum",
                "Keyword",
                "Snippet",
                "Color",
                "File",
                "Reference",
                "Folder",
                "EnumMember",
                "Constant",
                "Struct",
                "Event",
                "Operator",
                "TypeParameter",
            });
    }
}
