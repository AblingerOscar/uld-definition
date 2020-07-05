using autosupport_lsp_server.Serialization;
using autosupport_lsp_server.Serialization.Annotation;
using autosupport_lsp_server.Symbols;
using autosupport_lsp_server.Symbols.Impl.Terminals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace autosupport_lsp_server
{
    [XLinqName("languageDefinition")]
    public class AutosupportLanguageDefinition : IAutosupportLanguageDefinition
    {
        private AutosupportLanguageDefinition()
        {
            LanguageId = "";
            LanguageFilePattern = "";
            CommentRules = new CommentRules();
            StartRules = new string[0];
            Rules = new Dictionary<string, IRule>();
        }

        public AutosupportLanguageDefinition(string languageId, string languageFilePattern, CommentRules commentRules, string[] startRules, IDictionary<string, IRule> rules)
        {
            LanguageId = languageId;
            LanguageFilePattern = languageFilePattern;
            CommentRules = commentRules;
            StartRules = startRules;
            Rules = rules;
        }

        [XLinqName("name")]
        public string LanguageId { get; private set; }
        [XLinqName("filePattern")]
        public string LanguageFilePattern { get; private set; }

        public CommentRules CommentRules { get; private set; }

        [XLinqName("startRules")]
        [XLinqValue("startRule")]
        public string[] StartRules { get; private set; }

        [XLinqName("rules")]
        public IDictionary<string, IRule> Rules { get; private set; }

        public string[] VerifyAndGetErrors()
        {
            var errors = new List<string>();

            foreach (var rule in Rules)
            {
                foreach (var symbol in rule.Value.Symbols)
                {
                    symbol.Match(
                        terminal =>
                        {
                            if (terminal is StringTerminal stringTerminal && stringTerminal.String == "")
                                errors.Add($"rule {rule.Key}: A string terminal is empty");
                        },
                        nonTerminal =>
                        {
                            if (!Rules.ContainsKey(nonTerminal.ReferencedRule))
                                errors.Add($"rule {rule.Key}: Referenced rule {nonTerminal.ReferencedRule} not defined");
                        },
                        action =>
                        {
                            string? actionError = GetErrorWithAction(action);

                            if (actionError != null)
                                errors.Add($"rule {rule.Key}: " + actionError);
                        },
                        oneOf =>
                        {
                            if (!oneOf.AllowNone && oneOf.Options.Length == 0)
                                errors.Add($"rule {rule.Key}: A OneOf does not allowNone and is empty");
                            if (oneOf.AllowNone && oneOf.Options.Length == 0)
                                errors.Add($"rule {rule.Key}: Useless OneOf: allowNone is true and no options are given");
                            if (!oneOf.AllowNone && oneOf.Options.Length == 1)
                                errors.Add($"rule {rule.Key}: Useless OneOf: allowNone is false and only one option is given");
                        }
                        );
                }
            }

            return errors.ToArray();
        }

        private string? GetErrorWithAction(IAction action)
        {
            return action.Command switch
            {
                IAction.IDENTIFIER => null,

                IAction.IDENTIFIER_TYPE =>
                    action.GetArguments()[0] switch
                    {
                        IAction.IDENTIFIER_TYPE_ARG_SET => null,
                        IAction.IDENTIFIER_TYPE_ARG_INNER => null,
                        _ => $"First argument {action.GetArguments()[0]} not supported for {action.Command}"
                    },

                IAction.IDENTIFIER_KIND =>
                    action.GetArguments()[0] switch
                    {
                        IAction.IDENTIFIER_KIND_ARG_SET =>
                            Constants.ValidKinds.Contains(action.GetArguments()[1])
                                ? null
                                : $"Kind {action.GetArguments()[1]} is not supported",
                        _ => $"First argument {action.GetArguments()[0]} not supported for {action.Command}"
                    },

                IAction.DECLARATION => null,

                IAction.IMPLEMENTATION => null,

                IAction.FOLDING =>
                    action.GetArguments()[0] switch
                    {
                        IAction.FOLDING_START => null,
                        IAction.FOLDING_END => null,
                        _ => $"First argument {action.GetArguments()[0]} not supported for {action.Command}"
                    },

                _ => $"Command {action.Command} is not supported"
            };
        }

        public XElement SerializeToXLinq()
        {
            return new XElement(annotation.ClassName(),
                new XAttribute(annotation.PropertyName(nameof(LanguageId)), LanguageId),
                new XAttribute(annotation.PropertyName(nameof(LanguageFilePattern)), LanguageFilePattern),
                CommentRules.SerializeToXLinq(),
                new XElement(annotation.PropertyName(nameof(StartRules)),
                    from node in StartRules
                    select new XElement(annotation.ValuesName(nameof(StartRules)), node)),
                new XElement(annotation.PropertyName(nameof(Rules)),
                    (from rule in Rules
                     select rule.Value.SerializeToXLinq()))
                );
        }

        private readonly static AnnotationUtils.XLinqClassAnnotationUtil annotation = AnnotationUtils.XLinqOf(typeof(AutosupportLanguageDefinition));

        public static AutosupportLanguageDefinition FromXLinq(XElement element, IInterfaceDeserializer interfaceDeserializer)
        {
            if (element == null)
                throw new System.ArgumentException("'element' is null");

            return new AutosupportLanguageDefinition()
            {
                LanguageId = element.Attribute(annotation.PropertyName(nameof(LanguageId))).Value,
                LanguageFilePattern = element.Attribute(annotation.PropertyName(nameof(LanguageFilePattern))).Value,
                CommentRules = interfaceDeserializer
                    .DeserializeCommentRules(element.Element(annotation.ClassName(typeof(CommentRules)))),
                StartRules = (from startRuleElement in element
                                        .Element(annotation.PropertyName(nameof(StartRules)))
                                        .Elements(annotation.ValuesName(nameof(StartRules)))
                              select startRuleElement.Value)
                              .ToArray(),
                Rules = (from ruleElement in element
                                        .Element(annotation.PropertyName(nameof(Rules)))
                                        .Elements()
                         select interfaceDeserializer.DeserializeRule(ruleElement))
                         .ToDictionary(rule => rule.Name)
            };
        }
    }
}
