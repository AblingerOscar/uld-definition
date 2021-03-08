using System;

namespace uld.definition.Serialization.Annotation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    internal class XLinqValueAttribute : Attribute
    {
        public string ValuesName { get; }

        public XLinqValueAttribute(string name)
        {
            ValuesName = name;
        }
    }
}
