using uld.definition.Serialization;
using System;

namespace uld.definition.Symbols
{
    public interface ISymbol : IXLinqSerializable
    {
        void Match(
            Action<ITerminal> terminal,
            Action<INonTerminal> nonTerminal,
            Action<IAction> action,
            Action<IOneOf> oneOf);
        R Match<R>(
            Func<ITerminal, R> terminal,
            Func<INonTerminal, R> nonTerminal,
            Func<IAction, R> action,
            Func<IOneOf, R> oneOf);
    }
}
