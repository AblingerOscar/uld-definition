using uld.definition.Serialization;
using uld.definition.Serialization.Annotation;
using System;
using System.Xml.Linq;

namespace uld.definition.Symbols.Impl
{
    [XLinqName("nonTerminal")]
    public class NonTerminal : Symbol, INonTerminal
    {
        [XLinqName("referencedRule")]
        public string ReferencedRule { get; private set; } = "";

        public NonTerminal() { }

        public NonTerminal(string referencedRule) => ReferencedRule = referencedRule;

        public override void Match(Action<ITerminal> terminal, Action<INonTerminal> nonTerminal, Action<IAction> action, Action<IOneOf> oneOf)
        {
            nonTerminal.Invoke(this);
        }

        public override R Match<R>(Func<ITerminal, R> terminal, Func<INonTerminal, R> nonTerminal, Func<IAction, R> action, Func<IOneOf, R> oneOf)
        {
            return nonTerminal.Invoke(this);
        }

        private static readonly AnnotationUtils.XLinqClassAnnotationUtil annotation = AnnotationUtils.XLinqOf(typeof(NonTerminal));

        public override XElement SerializeToXLinq()
        {
            var element = base.SerializeToXLinq();

            element.SetAttributeValue(
                    annotation.PropertyName(nameof(ReferencedRule)),
                    ReferencedRule
                );

            return element;
        }

        public static NonTerminal FromXLinq(XElement element, IInterfaceDeserializer interfaceDeserializer)
        {
            var symbol = new NonTerminal()
            {
                ReferencedRule = element.Attribute(annotation.PropertyName(nameof(ReferencedRule))).Value
            };

            AddSymbolValuesFromXLinq(symbol, element, interfaceDeserializer);

            return symbol;
        }

        public override string? ToString()
        {
            return $"NonTerminal({ReferencedRule})";
        }
    }
}
