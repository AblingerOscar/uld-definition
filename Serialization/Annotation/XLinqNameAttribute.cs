﻿using System;

namespace uld.definition.Serialization.Annotation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, Inherited = true)]
    internal class XLinqNameAttribute : Attribute
    {
        public string Name { get; }

        public XLinqNameAttribute(string name)
        {
            Name = name;
        }
    }
}
