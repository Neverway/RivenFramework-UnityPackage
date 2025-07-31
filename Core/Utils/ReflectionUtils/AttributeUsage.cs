using System;
using System.Reflection;

namespace RivenFramework.Utils.Reflection
{
    public struct AttributeUsage
    {
        public MemberInfo Member { get; private set; }
        public Attribute Attribute { get; private set; }
        public Type Type => Attribute.GetType();
        public bool IsInherited { get; private set; }

        public AttributeUsage(MemberInfo member, Attribute attribute, bool isInherited)
        {
            Member = member;
            Attribute = attribute;
            IsInherited = isInherited;
        }

        public bool Is<TType>() => typeof(TType).IsAssignableFrom(Type);
        public TType As<TType>()
        {
            if (Attribute is TType CastedAttribute)
                return CastedAttribute;
            throw new InvalidCastException($"{Type.Name} is not of type {typeof(TType)}");
        }

    }
    public struct AttributeUsage<TAttribute> where TAttribute : Attribute
    {

        public AttributeUsage(AttributeUsage usage)
        {

        }
    }
}
