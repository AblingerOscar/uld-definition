using uld.definition.Serialization;
using uld.definition.Serialization.Annotation;
using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using static uld.definition.Serialization.Annotation.AnnotationUtils;

namespace uld.definition.Symbols.Impl
{
    [XLinqName("oneOf")]
    public class OneOf : IOneOf
    {
        [XLinqKeys("option")]
        public string[] Options { get; private set; } = new string[0];

        [XLinqName("allowNone")]
        public bool AllowNone { get; private set; } = false;

        public OneOf() { }

        public OneOf(bool allowNone, string[] options)
        {
            AllowNone = allowNone;
            Options = options;
        }

        public void Match(Action<ITerminal> terminal, Action<INonTerminal> nonTerminal, Action<IAction> action, Action<IOneOf> oneOf)
        {
            oneOf.Invoke(this);
        }

        public R Match<R>(Func<ITerminal, R> terminal, Func<INonTerminal, R> nonTerminal, Func<IAction, R> action, Func<IOneOf, R> oneOf)
        {
            return oneOf.Invoke(this);
        }

        public XElement SerializeToXLinq()
        {
            return new XElement(annotation.ClassName(),
                new[] { new XAttribute(annotation.PropertyName(nameof(AllowNone)), XmlConvert.ToString(AllowNone)) }
                .Union(
                    Options.Select<string, object>(option =>
                        new XElement(annotation.KeysName(nameof(Options)), option)))
                .ToArray());
        }

        public static IOneOf FromXLinq(XElement element, IInterfaceDeserializer interfaceDeserializer)
        {
            return new OneOf()
            {
                AllowNone = XmlConvert.ToBoolean(element.Attribute(annotation.PropertyName(nameof(AllowNone))).Value),
                Options = (from optionXml in element.Elements(annotation.KeysName(nameof(Options)))
                           select optionXml.Value).ToArray()
            };
        }

        public override string? ToString()
        {
            return new StringBuilder()
                .Append("OneOf(")
                .AppendJoin(", ", Options)
                .Append(')')
                .If(AllowNone, sb => sb.Append('?'))
                .ToString();
        }

        private static readonly XLinqClassAnnotationUtil annotation = AnnotationUtils.XLinqOf(typeof(OneOf));
    }
}
