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
    // rename autosupport to ULD = "Universal Language Definition"

    [XLinqName("languageDefinition")]
    public class LanguageDefinition : ILanguageDefinition
    {
        private LanguageDefinition()
        {
            LanguageId = "";
            LanguageFilePattern = "";
            CommentRules = new CommentRules();
            StartRules = new string[0];
            Rules = new Dictionary<string, IRule>();
        }

        public LanguageDefinition(string languageId, string languageFilePattern, CommentRules commentRules, string[] startRules, IDictionary<string, IRule> rules)
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
            // TODO: check for unused rules (aka not referenced in either a nonTerminal or a startRule)
            // TODO: check for duplicate characters in characterOf or characterExcept

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(LanguageId))
                errors.Add($"{nameof(LanguageId)} is null or an empty string");

            if (string.IsNullOrWhiteSpace(LanguageFilePattern))
                errors.Add($"{nameof(LanguageFilePattern)} is null or an empty string");

            if (CommentRules.DocumentationComments == null)
                errors.Add($"{nameof(CommentRules)}.{nameof(CommentRules.DocumentationComments)} is null (empty array is allowed)");

            if (CommentRules.NormalComments == null)
                errors.Add($"{nameof(CommentRules)}.{nameof(CommentRules.NormalComments)} is null (empty array is allowed)");

            new[] { CommentRules.DocumentationComments, CommentRules.NormalComments }
                .WhereNotNull()
                .SelectMany(comments => comments)
                .ForEach(comment =>
                {
                    if (comment.Start == null || comment.End == null || comment.TreatAs == null)
                        errors.Add("A comment's start, end, treatAs or any combination of those is null");

                    var nonWsTreatAsCharacters = comment.TreatAs?
                        .Where(ch => !char.IsWhiteSpace(ch))
                        .ToArray();

                    if (nonWsTreatAsCharacters != null && nonWsTreatAsCharacters?.Length != 0)
                        errors.Add($"{comment}: invalid characters {nonWsTreatAsCharacters.JoinToString(", ")} in TreatAs – only whitespace is allowed");
                });

            if (StartRules == null)
                errors.Add($"{nameof(StartRules)} is null");
            else if (StartRules.Length == 0)
                errors.Add($"{nameof(StartRules)}: No rule was specified");
            else
                foreach (var rule in StartRules)
                    if (Rules == null || !Rules.ContainsKey(rule))
                        errors.Add($"{nameof(StartRules)}: Startrule '{rule}' was not found in {nameof(Rules)}");

            if (Rules == null || Rules.Count == 0)
                errors.Add($"{nameof(Rules)} is null or empty");
            else
            {
                foreach (var rule in Rules)
                {
                    if (rule.Value.Symbols == null)
                    {
                        errors.Add($"{rule.Key}'s symbols are null");
                        continue;
                    }
                    // TODO: properly check for left-recursion even if other elements but nonTerminal are the first
                    //     element (e.g. actions or oneOf)
                    rule.Value.Symbols[0].Match(
                        _ => { },
                        nonTerminal => {
                            if (nonTerminal.ReferencedRule == rule.Key)
                                errors.Add($"{rule.Key}: left-recursion detected (according to the specifications this does not have to be supported by the server)");
                        },
                        _ => { },
                        _ => { });

                    foreach (var symbol in rule.Value.Symbols)
                    {
                        symbol.Match(
                            terminal =>
                            {
                                switch (terminal)
                                {
                                    case StringTerminal stringTerminal when stringTerminal.String == "":
                                        errors.Add($"rule {rule.Key}: A string terminal is empty");
                                        break;
                                    case AnyCharExceptTerminal anyCharExceptTerminal when anyCharExceptTerminal.Chars.Length == 0:
                                        errors.Add($"rule {rule.Key}: A AnyCharExceptTerminal has no options");
                                        break;
                                    case OneCharOfTerminal oneCharOfTerminal when oneCharOfTerminal.Chars.Length == 0:
                                        errors.Add($"rule {rule.Key}: A OneCharOf has no options");
                                        break;
                                }
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
            }

            return errors.ToArray();
        }

        private string? GetErrorWithAction(IAction action)
        {
            return action.GetBaseCommand() switch
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

                IAction.DEFINITION => null,

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
                new XAttribute(XName.Get("version", Constants.XML_NAMESPACE), "v1.0.0"),
                CommentRules.SerializeToXLinq(),
                new XElement(annotation.PropertyName(nameof(StartRules)),
                    from node in StartRules
                    select new XElement(annotation.ValuesName(nameof(StartRules)), node)),
                new XElement(annotation.PropertyName(nameof(Rules)),
                    (from rule in Rules
                     select rule.Value.SerializeToXLinq()))
                );
        }

        private readonly static AnnotationUtils.XLinqClassAnnotationUtil annotation = AnnotationUtils.XLinqOf(typeof(LanguageDefinition));

        public static LanguageDefinition FromXLinq(XElement element, IInterfaceDeserializer interfaceDeserializer)
        {
            if (element == null)
                throw new System.ArgumentException("'element' is null");

            return new LanguageDefinition()
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
