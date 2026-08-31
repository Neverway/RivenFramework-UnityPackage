using System;
using System.Reflection;
using Unity.VisualScripting;

namespace ErryLib.Reflection
{
    public class AttributeInfo
    {
        public MemberInfo Member { get; private set; }
        public Attribute Attribute { get; private set; }
        public Type Type => Attribute.GetType();
        public bool IsInherited { get; private set; }
        public bool IsOnClass => Member.MemberType == MemberTypes.TypeInfo;
        public bool IsOnMethod => Member.MemberType == MemberTypes.Method;
        public bool IsOnField => Member.MemberType == MemberTypes.Field;
        public bool IsOnProperty => Member.MemberType == MemberTypes.Property;
        public bool IsOnConstructor => Member.MemberType == MemberTypes.Constructor;

        public AttributeInfo(MemberInfo member, Attribute attribute, bool isInherited)
        {
            Member = member;
            Attribute = attribute;
            IsInherited = isInherited;
        }

        public bool Is<TAttributeType>() where TAttributeType : Attribute 
            => typeof(TAttributeType).IsAssignableFrom(Type);
        public TAttributeType As<TAttributeType>() where TAttributeType : Attribute
        {
            if (Attribute is TAttributeType _castedAttribute)
                return _castedAttribute;

            throw new InvalidCastException($"{Type.Name} is not of type {typeof(TAttributeType)}");
        }
        public bool GetAttribute<TAttributeType>(out TAttributeType attribute) where TAttributeType : Attribute
        {
            if (Attribute is TAttributeType _castedAttribute)
            {
                attribute = _castedAttribute;
                return true;
            }
            attribute = null;
            return false;
        }
        public bool GetMember<TMemberType>(out TMemberType member) where TMemberType : MemberInfo
        {
            if (Member is TMemberType _castedMember)
            {
                member = _castedMember;
                return true;
            }
            member = null;
            return false;
        }
    }
}
