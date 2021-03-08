using uld.definition.Serialization;
using uld.definition.Serialization.Annotation;
using System;
using System.Xml.Linq;

namespace uld.definition.Symbols.Impl
{
    public abstract class Symbol : ISymbol
    {
        public abstract void Match(Action<ITerminal> terminal, Action<INonTerminal> nonTerminal, Action<IAction> action, Action<IOneOf> oneOf);

        public abstract R Match<R>(Func<ITerminal, R> terminal, Func<INonTerminal, R> nonTerminal, Func<IAction, R> action, Func<IOneOf, R> oneOf);

        public virtual XElement SerializeToXLinq()
        {
            return new XElement(annotation.RuntimeClassName(GetType()));
        }

        protected static void AddSymbolValuesFromXLinq(Symbol symbol, XElement element, IInterfaceDeserializer interfaceDeserializer)
        {
            // no properties
        }

        private static readonly AnnotationUtils.XLinqClassAnnotationUtil annotation = AnnotationUtils.XLinqOf(typeof(Symbol));

        public override string? ToString()
        {
            return $"{annotation.RuntimeClassName(GetType())}";
        }
    }
}